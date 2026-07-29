// @ts-nocheck
"use client"

import type React from "react"
import { useState, useEffect, useCallback } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { PlusCircle, Loader2, Trash2, ChevronDown, ChevronUp, Info, AlertTriangle, Settings, Save, Eye } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { createWorkflow, getWorkflows } from "@/services/workflow.service"
import { getProducts } from "@/services/product.service"
import { getDeviceModels } from "@/services/device.service"
import InfiniteScroll from "react-infinite-scroll-component"
import type { Product } from "@/interfaces/product"
import type { DeviceModel } from "@/interfaces/device"
import type { Workflow } from "@/interfaces/workflow"
import type { ErrorResponse } from "@/types/error"
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { EWorkflowType, EWorkflowTypeViMap, EExpressionType, EOperation, EConditionName, EExpressionTypeViMap, EOperationViMap, EConditionNameViMap, EOperationMap } from "@/enum/workflow"
import { workflowSchema } from "@/schema/workflow.schema"
import { ZodError } from "zod"
import { useRouter } from "next/navigation"
import { Path } from "@/constants/path.constant"
import type { KioskVersion } from "@/interfaces/kiosk"
import { getKioskVersions } from "@/services/kiosk.service"
import { FunctionParameterEditor } from "@/components/common"
import { getLuaScripts, type LuaScript } from "@/services/luaScript.service"
import ReactFlow, {
    addEdge,
    Background,
    BackgroundVariant,
    type Connection,
    Controls,
    type Edge,
    type Node,
    type NodeTypes,
    Position,
    useEdgesState,
    useNodesState,
} from "reactflow"
import "reactflow/dist/style.css"
import WorkflowStepNode from "@/components/common/workflow-step-node"

const styles = `
  .react-flow__attribution {
    display: none;
  }
  .react-flow__node-workflowStep .react-flow__handle-left,
  .react-flow__node-workflowStep .react-flow__handle-right {
    display: flex;
  }
  .react-flow__handle-top { top: -20px; width: 15px; height: 15px; }
  .react-flow__handle-bottom { bottom: -20px; width: 15px; height: 15px; }
  .react-flow__handle.hidden {
    display: none;
  }
`

const initialFormData = {
    name: "",
    description: "",
    type: EWorkflowType.Activity,
    productId: null,
    steps: [
        {
            stepCode: Math.random().toString(36).substring(2, 14),
            name: "Bước 1",
            type: "",
            deviceModelId: "",
            deviceFunctionId: "",
            maxRetries: 0,
            sequence: 1,
            callbackWorkflowId: "",
            callbackStepCode: null,
            parameters: "",
            conditions: [],
            system: "arm" as "arm" | "iot",
            timeout: 20,
            // Request data
            luaFile: "", // For arm
            iotCommand: "send_hex", // For iot
            iotDevice: "", // For iot
            iotHex: "", // For iot
            iotFlush: true, // For iot
            iotReadLen: 16, // For iot
        },
    ],
}

const nodeTypes: NodeTypes = {
    workflowStep: WorkflowStepNode,
}

const generateUniqueStepCode = (existingSteps: any[]): string => {
    const existingCodes = new Set(existingSteps.map(s => s.stepCode));
    let newCode: string;
    do {
        newCode = Math.random().toString(36).substring(2, 14);
    } while (existingCodes.has(newCode));
    return newCode;
};

const CreateWorkflow = () => {
    const router = useRouter()
    const { toast } = useToast()
    const [errors, setErrors] = useState<Record<string, any>>({})
    const [loading, setLoading] = useState<boolean>(false)
    const [formData, setFormData] = useState(initialFormData)
    const [showWorkflowInfo, setShowWorkflowInfo] = useState(false)
    const [isConfirming, setIsConfirming] = useState(false)

    const [products, setProducts] = useState<Product[]>([])
    const [productPage, setProductPage] = useState<number>(1)
    const [hasMoreProducts, setHasMoreProducts] = useState(true)
    const [loadingProducts, setLoadingProducts] = useState(true)

    const [deviceModels, setDeviceModels] = useState<DeviceModel[]>([])
    const [deviceModelPage, setDeviceModelPage] = useState<number>(1)
    const [hasMoreDeviceModels, setHasMoreDeviceModels] = useState(true)
    const [loadingDeviceModels, setLoadingDeviceModels] = useState(false)

    const [workflows, setWorkflows] = useState<Workflow[]>([])
    const [workflowPage, setWorkflowPage] = useState<number>(1)
    const [hasMoreWorkflows, setHasMoreWorkflows] = useState(true)
    const [loadingWorkflows, setLoadingWorkflows] = useState(true)

    const [kioskVersions, setKioskVersions] = useState<KioskVersion[]>([])
    const [kioskVersionPage, setKioskVersionPage] = useState(1)
    const [hasMoreKioskVersion, setHasMoreKioskVersion] = useState(true)
    const [selectedKioskVersion, setSelectedKioskVersion] = useState<string>("")
    const [loadingKioskVersions, setLoadingKioskVersions] = useState(true)
    const [kioskVersionError, setKioskVersionError] = useState<string | null>(null)

    const [luaScripts, setLuaScripts] = useState<LuaScript[]>([])
    const [loadingLuaScripts, setLoadingLuaScripts] = useState(false)

    const [editingStepIndex, setEditingStepIndex] = useState<number | null>(null)

    const [nodes, setNodes, onNodesChange] = useNodesState([])
    const [edges, setEdges, onEdgesChange] = useEdgesState([])

    const fetchKioskVersions = useCallback(
        async (pageNumber: number) => {
            if (pageNumber === 1) setLoadingKioskVersions(true)
            setKioskVersionError(null)
            try {
                const response = await getKioskVersions({ page: pageNumber, size: 10 })
                if (!response || !response.items) {
                    throw new Error("Invalid response format for Kiosk Versions")
                }
                setKioskVersions((prev) => (pageNumber === 1 ? response.items : [...prev, ...response.items]))
                setKioskVersionPage(pageNumber)
                setHasMoreKioskVersion(response.items.length >= 10)
            } catch (error) {
                const err = error as ErrorResponse
                console.error("Error fetching kiosk versions:", error)
                setKioskVersionError(err.message || "Lỗi khi tải phiên bản kiosk")
            } finally {
                if (pageNumber === 1 || !hasMoreKioskVersion) setLoadingKioskVersions(false)
            }
        },
        [toast]
    )

    const loadMoreKioskVersions = useCallback(async () => {
        if (loadingKioskVersions || !hasMoreKioskVersion) return
        await fetchKioskVersions(kioskVersionPage + 1)
    }, [loadingKioskVersions, hasMoreKioskVersion, kioskVersionPage, fetchKioskVersions])

    const fetchProducts = useCallback(
        async (pageNumber: number) => {
            if (pageNumber === 1) setLoadingProducts(true)
            try {
                const response = await getProducts({ page: pageNumber, size: 100, isHasWorkflow: false, productType: "Child" })
                setProducts((prev) => (pageNumber === 1 ? response.items : [...prev, ...response.items]))
                setProductPage(pageNumber)
                setHasMoreProducts(response.items.length >= 10)
            } catch (error) {
                console.error("Error fetching products:", error)
                toast({ title: "Lỗi", description: "Không tải được danh sách sản phẩm.", variant: "destructive" })
            } finally {
                if (pageNumber === 1 || !hasMoreProducts) setLoadingProducts(false)
            }
        },
        [toast]
    )

    const fetchDeviceModels = useCallback(
        async (pageNumber: number) => {
            if (!selectedKioskVersion) return
            if (pageNumber === 1) setLoadingDeviceModels(true)
            try {
                const response = await getDeviceModels({ kioskVersionId: selectedKioskVersion, page: pageNumber, size: 10 })
                setDeviceModels((prev) => (pageNumber === 1 ? response.items : [...prev, ...response.items]))
                setDeviceModelPage(pageNumber)
                setHasMoreDeviceModels(response.items.length >= 10)
            } catch (error) {
                console.error("Error fetching device models:", error)
                toast({ title: "Lỗi", description: "Không tải được các loại thiết bị.", variant: "destructive" })
            } finally {
                if (pageNumber === 1 || !hasMoreDeviceModels) setLoadingDeviceModels(false)
            }
        },
        [selectedKioskVersion, toast]
    )

    const fetchWorkflows = useCallback(
        async (pageNumber: number) => {
            if (pageNumber === 1) setLoadingWorkflows(true)
            try {
                const response = await getWorkflows({ page: pageNumber, size: 10 })
                setWorkflows((prev) => (pageNumber === 1 ? response.items : [...prev, ...response.items]))
                setWorkflowPage(pageNumber)
                setHasMoreWorkflows(response.items.length >= 10)
            } catch (error) {
                console.error("Error fetching workflows:", error)
                toast({ title: "Lỗi", description: "Không tải được các quy trình.", variant: "destructive" })
            } finally {
                if (pageNumber === 1 || !hasMoreWorkflows) setLoadingWorkflows(false)
            }
        },
        [toast]
    )

    const fetchLuaScripts = useCallback(async () => {
        setLoadingLuaScripts(true)
        try {
            const scripts = await getLuaScripts()
            setLuaScripts(scripts)
        } catch (error) {
            console.error("Error fetching Lua scripts:", error)
            toast({ title: "Lỗi", description: "Không tải được danh sách Lua scripts.", variant: "destructive" })
        } finally {
            setLoadingLuaScripts(false)
        }
    }, [toast])

    useEffect(() => {
        fetchProducts(1)
        fetchWorkflows(1)
        fetchKioskVersions(1)
        fetchLuaScripts()
    }, [fetchProducts, fetchWorkflows, fetchKioskVersions, fetchLuaScripts])

    useEffect(() => {
        if (selectedKioskVersion) {
            setDeviceModels([])
            setDeviceModelPage(1)
            setHasMoreDeviceModels(true)
            fetchDeviceModels(1)
        } else {
            setDeviceModels([])
        }
    }, [selectedKioskVersion, fetchDeviceModels])

    const getDeviceFunctionsForModel = useCallback(
        (deviceModelId: string) => {
            const deviceModel = deviceModels.find((dm) => dm.deviceModelId === deviceModelId)
            return deviceModel?.deviceFunctions || []
        },
        [deviceModels]
    )

    const handleChange = useCallback(
        (field: string, value: string | null) => {
            setFormData((prev) => ({
                ...prev,
                [field]: value,
            }))
            if (errors[field]) {
                setErrors((prev) => {
                    const newErrors = { ...prev }
                    delete newErrors[field]
                    return newErrors
                })
            }
        },
        [errors]
    )

    const handleStepChange = useCallback(
        (index: number, field: string, value: string | number | null) => {
            setFormData((prev) => {
                const newSteps = [...prev.steps]
                const currentStep = { ...newSteps[index] }

                if (field === "maxRetries" || field === "sequence") {
                    // Cho phép để trống hoặc chỉ nhận số >= 0
                    if (value === "" || /^\d+$/.test(value.toString())) {
                        currentStep[field] = value === "" ? "" : Number(value);
                    } else {
                        currentStep[field] = value; // để schema báo lỗi
                    }
                } else if (field === "name") {
                    currentStep.name = String(value || "")
                } else if (field === "deviceModelId") {
                    currentStep.deviceModelId = (value as string) || ""
                    currentStep.deviceFunctionId = ""
                    currentStep.type = ""
                    currentStep.parameters = ""
                    // Reset IoT fields khi đổi device model
                    currentStep.iotDevice = ""
                    currentStep.iotHex = ""
                    // Tự động xác định System từ DeviceType
                    if (value) {
                        const deviceModel = deviceModels.find((dm) => dm.deviceModelId === value)
                        if (deviceModel) {
                            // Kiểm tra tên DeviceType hoặc ModelName để xác định System
                            const modelName = deviceModel.modelName?.toLowerCase() || ""
                            const deviceTypeName = deviceModel.deviceType?.name?.toLowerCase() || ""
                            // Nếu có "arm", "robot", "fr5" trong tên → "arm", còn lại → "iot"
                            if (modelName.includes("arm") || modelName.includes("robot") || modelName.includes("fr5") ||
                                deviceTypeName.includes("arm") || deviceTypeName.includes("robot")) {
                                currentStep.system = "arm"
                            } else {
                                currentStep.system = "iot"
                                // Tự động điền device name từ modelName cho IoT
                                if (deviceModel.modelName) {
                                    currentStep.iotDevice = deviceModel.modelName
                                }
                            }
                        }
                    }
                } else if (field === "deviceFunctionId") {
                    currentStep.deviceFunctionId = (value as string) || ""
                    currentStep.parameters = ""
                    if (currentStep.deviceFunctionId && currentStep.deviceModelId) {
                        const deviceModel = deviceModels.find((dm) => dm.deviceModelId === currentStep.deviceModelId)
                        const selectedFunction = deviceModel?.deviceFunctions?.find(
                            (df) => df.deviceFunctionId === currentStep.deviceFunctionId || df.name === currentStep.deviceFunctionId
                        )
                        if (selectedFunction && typeof selectedFunction.name === "string" && selectedFunction.name.trim() !== "") {
                            currentStep.type = selectedFunction.name.trim()
                        } else {
                            currentStep.type = ""
                        }

                        // Tự động điền thông tin cho IoT devices
                        if (currentStep.system === "iot" && deviceModel && selectedFunction) {
                            // Tự động điền device name từ modelName
                            if (deviceModel.modelName && !currentStep.iotDevice) {
                                // Map modelName thành device name (ví dụ: "IceMaker" → "IceMake")
                                const modelName = deviceModel.modelName
                                // Có thể cần mapping logic ở đây, tạm thời dùng modelName
                                currentStep.iotDevice = modelName
                            }

                            // Tìm hex code trong functionParameters
                            if (selectedFunction.functionParameters && selectedFunction.functionParameters.length > 0) {
                                // Tìm parameter có tên chứa "hex" hoặc có default value là hex code
                                let hexFound = false
                                
                                selectedFunction.functionParameters.forEach((param) => {
                                    const paramName = param.name?.toLowerCase() || ""
                                    const defaultValue = (param.default || "").toString().trim()
                                    
                                    // Kiểm tra nếu tên parameter chứa "hex" hoặc "code"
                                    if ((paramName.includes("hex") || paramName.includes("code")) && defaultValue) {
                                        // Nếu default value có format hex (chứa 0-9, A-F, a-f và khoảng trắng)
                                        if (/^[0-9A-Fa-f\s]+$/.test(defaultValue) && defaultValue.length > 0) {
                                            currentStep.iotHex = defaultValue
                                            hexFound = true
                                        }
                                    }
                                    
                                    // Nếu chưa tìm thấy hex, kiểm tra tất cả default values có format hex
                                    if (!hexFound && defaultValue && /^[0-9A-Fa-f\s]{3,}$/.test(defaultValue)) {
                                        // Nếu có ít nhất 3 ký tự hex (ví dụ: "04 07 AA")
                                        currentStep.iotHex = defaultValue
                                        hexFound = true
                                    }
                                    
                                    // Tìm các thông tin khác nếu có
                                    if (paramName.includes("flush") && param.default !== undefined) {
                                        currentStep.iotFlush = param.default === "true" || param.default === true || param.default === "1"
                                    }
                                    if ((paramName.includes("read") || paramName.includes("length") || paramName.includes("len")) && param.default) {
                                        const readLen = Number.parseInt(param.default)
                                        if (!isNaN(readLen)) {
                                            currentStep.iotReadLen = readLen
                                        }
                                    }
                                    if (paramName.includes("device") && param.default) {
                                        // Nếu có parameter "device", có thể dùng làm device name
                                        if (!currentStep.iotDevice || currentStep.iotDevice === deviceModel.modelName) {
                                            currentStep.iotDevice = param.default.toString()
                                        }
                                    }
                                })
                            }
                        }

                        // Tự động điền Lua file cho Arm devices
                        if (currentStep.system === "arm" && selectedFunction) {
                            if (selectedFunction.luaScriptFileName && !currentStep.luaFile) {
                                currentStep.luaFile = selectedFunction.luaScriptFileName
                            }
                        }
                    }
                } else if (field === "parameters") {
                    currentStep.parameters = (value as string) || ""
                } else if (field === "system") {
                    currentStep.system = (value as "arm" | "iot") || "arm"
                } else if (field === "timeout") {
                    currentStep.timeout = value === "" ? 20 : Number(value)
                } else if (field === "luaFile") {
                    currentStep.luaFile = (value as string) || ""
                } else if (field === "iotCommand") {
                    currentStep.iotCommand = (value as string) || "send_hex"
                } else if (field === "iotDevice") {
                    currentStep.iotDevice = (value as string) || ""
                } else if (field === "iotHex") {
                    currentStep.iotHex = (value as string) || ""
                } else if (field === "iotFlush") {
                    currentStep.iotFlush = value === true || value === "true" || value === 1
                } else if (field === "iotReadLen") {
                    currentStep.iotReadLen = value === "" ? 16 : Number(value)
                } else if (field === "callbackStepCode") {
                    currentStep.callbackStepCode = value === "" ? null : value
                } else {
                    currentStep[field] = value
                    if (field === "callbackWorkflowId") {
                        currentStep.callbackWorkflowId = value === "" ? null : value
                    }
                }

                newSteps[index] = currentStep
                return { ...prev, steps: newSteps }
            })

            if (errors.steps?.[index]?.[field]) {
                setErrors((prevErrors) => {
                    const newErrors = { ...prevErrors }
                    if (newErrors.steps && newErrors.steps[index]) {
                        delete newErrors.steps[index][field]
                        if (Object.keys(newErrors.steps[index]).length === 0) {
                            delete newErrors.steps[index]
                            if (Object.keys(newErrors.steps).length === 0) {
                                delete newErrors.steps
                            }
                        }
                    }
                    return newErrors
                })
            }
        },
        [deviceModels, errors]
    )

    const addStep = useCallback(() => {
        setFormData((prev) => {
            const newStepCode = generateUniqueStepCode(prev.steps)
            const newSequence = prev.steps.length > 0 ? Math.max(...prev.steps.map(s => s.sequence)) + 1 : 1
            return {
                ...prev,
                steps: [
                    ...prev.steps,
                    {
                        stepCode: newStepCode,
                        name: `Bước ${newSequence}`,
                        type: "",
                        deviceModelId: "",
                        deviceFunctionId: "",
                        maxRetries: 0,
                        sequence: newSequence,
                        callbackWorkflowId: "",
                        callbackStepCode: null,
                        parameters: "",
                        conditions: [],
                        system: "arm" as "arm" | "iot",
                        timeout: 20,
                        // Request data
                        luaFile: "", // For arm
                        iotCommand: "send_hex", // For iot
                        iotDevice: "", // For iot
                        iotHex: "", // For iot
                        iotFlush: true, // For iot
                        iotReadLen: 16, // For iot
                    },
                ],
            }
        })
    }, [])

    const removeStep = useCallback(
        (index: number) => {
            setFormData((prev) => {
                const removedStepCode = prev.steps[index].stepCode
                const newSteps = prev.steps
                    .filter((_, i) => i !== index)
                    .map(step => {
                        if (step.callbackStepCode === removedStepCode) {
                            return { ...step, callbackStepCode: null }
                        }
                        return step
                    })
                return { ...prev, steps: newSteps }
            })
            if (editingStepIndex === index) setEditingStepIndex(null)
            else if (editingStepIndex && editingStepIndex > index) setEditingStepIndex(editingStepIndex - 1)
        },
        [editingStepIndex]
    )

    const moveStepUp = useCallback((index: number) => {
        if (index === 0) return
        setFormData((prev) => {
            const newSteps = [...prev.steps]
            const temp = newSteps[index]
            newSteps[index] = newSteps[index - 1]
            newSteps[index - 1] = temp
            return { ...prev, steps: newSteps }
        })
        setEditingStepIndex(index - 1)
    }, [])

    const moveStepDown = useCallback(
        (index: number) => {
            if (index === formData.steps.length - 1) return
            setFormData((prev) => {
                const newSteps = [...prev.steps]
                const temp = newSteps[index]
                newSteps[index] = newSteps[index + 1]
                newSteps[index + 1] = temp
                return { ...prev, steps: newSteps }
            })
            setEditingStepIndex(index + 1)
        },
        [formData.steps.length]
    )

    const handleConditionChange = useCallback(
        (stepIndex: number, conditionIndex: number, field: string, value: any) => {
            setFormData((prev) => {
                const newSteps = [...prev.steps]
                const currentStep = { ...newSteps[stepIndex] }
                const newConditions = [...(currentStep.conditions || [])]
                const currentCondition = { ...newConditions[conditionIndex] }

                if (field === "name") {
                    currentCondition.name = value
                } else if (field === "description") {
                    currentCondition.description = value
                } else if (field === "expression") {
                    currentCondition.expression = value
                } else if (field === "leftType") {
                    currentCondition.expression.left.type = value
                } else if (field === "leftValue") {
                    currentCondition.expression.left.value = value
                } else if (field === "operator") {
                    currentCondition.expression.operator = value
                } else if (field === "rightType") {
                    currentCondition.expression.right.type = value
                } else if (field === "rightValue") {
                    currentCondition.expression.right.value = value
                }

                newConditions[conditionIndex] = currentCondition
                currentStep.conditions = newConditions
                newSteps[stepIndex] = currentStep
                return { ...prev, steps: newSteps }
            })
        },
        []
    )

    const addCondition = useCallback((stepIndex: number) => {
        setFormData((prev) => {
            const newSteps = [...prev.steps]
            const currentStep = { ...newSteps[stepIndex] }
            const newConditions = [
                ...(currentStep.conditions || []),
                {
                    name: EConditionName.Side,
                    description: "",
                    expression: {
                        left: { type: EExpressionType.Variable, value: "" },
                        operator: EOperation.Equal,
                        right: { type: EExpressionType.Literal, value: "" },
                    },
                },
            ]
            currentStep.conditions = newConditions
            newSteps[stepIndex] = currentStep
            return { ...prev, steps: newSteps }
        })
    }, [])

    const removeCondition = useCallback((stepIndex: number, conditionIndex: number) => {
        setFormData((prev) => {
            const newSteps = [...prev.steps]
            const currentStep = { ...newSteps[stepIndex] }
            const newConditions = (currentStep.conditions || []).filter((_, i) => i !== conditionIndex)
            currentStep.conditions = newConditions
            newSteps[stepIndex] = currentStep
            return { ...prev, steps: newSteps }
        })
    }, [])

    const parseErrors = (error: ZodError) => {
        const errors: Record<string, any> = {};

        error.errors.forEach((e) => {
            const path = e.path; // ví dụ: ["steps", 0, "maxRetries"]
            let current: any = errors;

            for (let i = 0; i < path.length; i++) {
                const key = path[i];

                if (i === path.length - 1) {
                    // node cuối cùng => push message
                    if (!current[key]) {
                        current[key] = [];
                    }
                    current[key].push(e.message);
                } else {
                    // chưa đến cuối => tạo object hoặc array lồng nhau
                    if (typeof path[i + 1] === "number") {
                        // phần tử tiếp theo là số => array
                        if (!Array.isArray(current[key])) {
                            current[key] = [];
                        }
                    } else {
                        // phần tử tiếp theo là string => object
                        if (!current[key]) {
                            current[key] = {};
                        }
                    }
                    current = current[key];
                }
            }
        });

        return errors;
    };

    const handleValidationAndPreview = useCallback(
        (e: React.FormEvent) => {
            e.preventDefault()
            e.stopPropagation()

            const sortedSteps = [...formData.steps].sort((a, b) => a.sequence - b.sequence)
            // Map từ sorted index về original index
            const sortedToOriginalIndexMap = sortedSteps.map(sortedStep => 
                formData.steps.findIndex(s => s.stepCode === sortedStep.stepCode)
            )
            
            const dataToValidate = { ...formData, steps: sortedSteps }
            const result = workflowSchema.safeParse(dataToValidate)

            if (!result.success) {
                const newErrors = parseErrors(result.error);
                
                // Map errors từ sorted index về original index
                if (newErrors.steps && Array.isArray(newErrors.steps)) {
                    const mappedStepsErrors: any[] = []
                    newErrors.steps.forEach((error: any, sortedIndex: number) => {
                        const originalIndex = sortedToOriginalIndexMap[sortedIndex]
                        if (originalIndex !== -1) {
                            mappedStepsErrors[originalIndex] = error
                        }
                    })
                    newErrors.steps = mappedStepsErrors
                }
                
                setErrors(newErrors)
                toast({
                    title: "Lỗi xác thực",
                    description: "Vui lòng kiểm tra lại thông tin đã nhập",
                    variant: "destructive",
                })
                if (newErrors.name || newErrors.type || newErrors.description) {
                    setShowWorkflowInfo(true)
                }
                return
            }

            setErrors({})
            setIsConfirming(true)
        },
        [formData, toast]
    )

    const handleCreateWorkflow = useCallback(
        async () => {
            setLoading(true)
            try {
                const result = workflowSchema.safeParse(formData)
                if (!result.success) {
                    throw new Error("Dữ liệu không hợp lệ, vui lòng kiểm tra lại.")
                }
                const validatedData = result.data
                const operationMap: Record<string, string> = {
                    [EOperation.Equal]: "Equal",
                    [EOperation.NotEqual]: "NotEqual",
                    [EOperation.GreaterThan]: "GreaterThan",
                    [EOperation.GreaterThanOrEqual]: "GreaterThanOrEqual",
                    [EOperation.LessThan]: "LessThan",
                    [EOperation.LessThanOrEqual]: "LessThanOrEqual",
                }

                const dataToSend = {
                    name: validatedData.name,
                    description: validatedData.description || undefined,
                    type: validatedData.type,
                    productId: validatedData.productId || null,
                    kioskVersionId: selectedKioskVersion || undefined,
                    steps: validatedData.steps.map((step) => {
                        // Build Request object dựa trên System type
                        // Validation đã đảm bảo dữ liệu đầy đủ, nên không cần check null/empty nữa
                        let requestObject: any = null
                        let requestJson: string | undefined = undefined

                        if (step.system === "arm") {
                            // ARM: { "type": "run_lua", "file": "..." }
                            // Validation đã đảm bảo luaFile có giá trị
                            if (step.luaFile && step.luaFile.trim().length > 0) {
                                requestObject = {
                                    type: "run_lua",
                                    file: step.luaFile.trim()
                                }
                                requestJson = JSON.stringify(requestObject)
                            }
                        } else if (step.system === "iot") {
                            // IOT: { "command": "...", "device": "...", "hex": "...", "flush": true, "read_len": 16 }
                            // Validation đã đảm bảo iotDevice và iotHex có giá trị
                            if (step.iotDevice && step.iotDevice.trim().length > 0 && 
                                step.iotHex && step.iotHex.trim().length > 0) {
                                requestObject = {
                                    command: step.iotCommand || "send_hex",
                                    device: step.iotDevice.trim(),
                                    hex: step.iotHex.trim(),
                                    flush: step.iotFlush !== false, // Default true
                                    read_len: step.iotReadLen || 16
                                }
                                requestJson = JSON.stringify(requestObject)
                            }
                        } else {
                            // Fallback: dùng parameters nếu có
                            if (step.parameters && step.parameters.trim().length > 0) {
                                try {
                                    const parsed = JSON.parse(step.parameters.trim())
                                    requestJson = JSON.stringify(parsed)
                                } catch {
                                    requestJson = step.parameters.trim()
                                }
                            }
                        }

                        // Nếu không có requestJson (không nên xảy ra vì đã có validation), throw error
                        if (!requestJson) {
                            throw new Error(
                                `Step "${step.name}" thiếu thông tin request. ` +
                                `ARM cần luaFile, IoT cần iotDevice và iotHex.`
                            )
                        }

                        return {
                            ...step,
                            system: step.system || "arm",
                            timeout: step.timeout || 20,
                            requestJson: requestJson,
                            parameters: step.parameters || undefined, // Giữ lại để tương thích
                            callbackWorkflowId: step.callbackWorkflowId || undefined,
                            callbackStepCode: step.callbackStepCode || null,
                            deviceModelId: step.deviceModelId || undefined,
                            deviceFunctionId: step.deviceFunctionId || undefined,
                            sequence: step.sequence || 1,
                            conditions: (step.conditions || []).map((condition) => ({
                                ...condition,
                                expression: {
                                    ...condition.expression,
                                    operator: operationMap[condition.expression.operator] || condition.expression.operator,
                                },
                            })),
                        }
                    }),
                }

                await createWorkflow(dataToSend as any)
                toast({
                    title: "Thành công",
                    description: "Thêm quy trình mới thành công",
                })
                setIsConfirming(false)
                router.push(Path.MANAGE_WORKFLOWS)
            } catch (error) {
                const err = error as ErrorResponse
                console.error("Lỗi khi xử lý quy trình:", error)
                toast({
                    title: "Lỗi khi xử lý quy trình",
                    description: err.message || "Đã xảy ra lỗi không mong muốn.",
                    variant: "destructive",
                })
            } finally {
                setLoading(false)
            }
        },
        [formData, selectedKioskVersion, toast, router]
    )


    const loadMoreProducts = useCallback(async () => {
        if (loadingProducts || !hasMoreProducts) return
        await fetchProducts(productPage + 1)
    }, [productPage, fetchProducts, loadingProducts, hasMoreProducts])

    const onConnect = useCallback(
        (params: Edge | Connection) => {
            const sourceNodeId = params.source
            const targetNodeId = params.target

            if (sourceNodeId === targetNodeId) {
                toast({
                    title: "Lỗi",
                    description: "Không thể tạo cạnh từ node đến chính nó.",
                    variant: "destructive",
                })
                return
            }

            const existingLoop = edges.some(
                (edge) => edge.source === targetNodeId && edge.target === sourceNodeId
            )

            if (existingLoop) {
                toast({
                    title: "Lỗi",
                    description: "Không thể tạo vòng lặp trực tiếp giữa hai bước.",
                    variant: "destructive",
                })
                return
            }

            const existingEdgesFromSource = edges.filter((edge) => edge.source === sourceNodeId)
            if (existingEdgesFromSource.length >= 1) {
                toast({
                    title: "Lỗi",
                    description: "Mỗi bước chỉ có thể có một kết nối ra.",
                    variant: "destructive",
                })
                return
            }

            setEdges((eds) => addEdge({ ...params, animated: true, style: { stroke: "#3b82f6" } }, eds))
        },
        [edges, setEdges, toast]
    )

    const updateFlowFromSteps = useCallback(() => {
        const sortedSteps = [...formData.steps].sort((a, b) => a.sequence - b.sequence)

        const sequenceGroups: Record<number, any[]> = {}
        sortedSteps.forEach((step, index) => {
            const originalIndex = formData.steps.findIndex(s => s.stepCode === step.stepCode);
            const sequence = step.sequence
            if (!sequenceGroups[sequence]) {
                sequenceGroups[sequence] = []
            }
            sequenceGroups[sequence].push({ ...step, originalIndex: originalIndex })
        })

        const sequences = Object.keys(sequenceGroups).map(Number).sort((a, b) => a - b)

        const newNodes: Node[] = []
        const newEdges: Edge[] = []

        let yOffset = 0
        sequences.forEach((sequence, seqIndex) => {
            const stepsInSequence = sequenceGroups[sequence]
            const xOffset = 100
            const y = yOffset

            stepsInSequence.forEach((step, stepIndex) => {
                const nodeId = `step-${step.originalIndex}`
                const x = xOffset + stepIndex * 250
                newNodes.push({
                    id: nodeId,
                    type: "workflowStep",
                    position: { x, y },
                    data: {
                        step,
                        onEdit: () => setEditingStepIndex(step.originalIndex),
                        onDelete: () => removeStep(step.originalIndex),
                        errors: errors.steps?.[step.originalIndex] || {},
                    },
                })

                if (seqIndex > 0) {
                    const prevSequence = sequences[seqIndex - 1]
                    const prevSteps = sequenceGroups[prevSequence]
                    prevSteps.forEach((prevStep) => {
                        const prevNodeId = `step-${prevStep.originalIndex}`
                        const sourceHasCallbackToTarget = prevStep.callbackStepCode === step.stepCode
                        const targetHasCallbackToSource = step.callbackStepCode === prevStep.stepCode

                        // Don't draw the regular edge if a callback edge already exists between these two
                        if (!sourceHasCallbackToTarget && !targetHasCallbackToSource) {
                            newEdges.push({
                                id: `edge-${prevNodeId}-${nodeId}`,
                                source: prevNodeId,
                                target: nodeId,
                                animated: true,
                                style: { stroke: "#3b82f6" },
                                zIndex: 1100,
                            })
                        }
                    })
                }
            })

            yOffset += 200
        })

        formData.steps.forEach((step, index) => {
            if (step.callbackStepCode) {
                const callbackStepIndex = formData.steps.findIndex(s => s.stepCode === step.callbackStepCode)
                if (callbackStepIndex !== -1) {
                    const sourceNodeId = `step-${index}`
                    const targetNodeId = `step-${callbackStepIndex}`
                    newEdges.push({
                        id: `callback-edge-${sourceNodeId}-${targetNodeId}`,
                        source: sourceNodeId,
                        target: targetNodeId,
                        animated: true,
                        type: 'smoothstep',
                        style: { stroke: "red", strokeWidth: 2 },
                        markerEnd: { type: "arrowclosed", color: "red" },
                        zIndex: 1000,
                        label: 'callback'
                    })
                }
            }
        })


        setNodes(newNodes)
        setEdges(newEdges)
    }, [formData, errors.steps, removeStep, setNodes, setEdges])

    useEffect(() => {
        updateFlowFromSteps()
    }, [updateFlowFromSteps])

    return (
        <div className="container mx-auto p-6 space-y-8">
            <style>{styles}</style>
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold flex items-center">
                    Tạo quy trình mới
                </h1>
                <Button type="button" disabled={loading} className="bg-primary hover:bg-primary-200" onClick={handleValidationAndPreview}>
                    {loading ? (
                        <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Đang xử lý...
                        </>
                    ) : (
                        <>
                            <Eye className="mr-2 h-4 w-4" />
                            Xem lại & Tạo
                        </>
                    )}
                </Button>
            </div>

            <Card className="mb-6">
                <CardHeader>
                    <CardTitle className="flex items-center">
                        <Settings className="mr-2 h-5 w-5" />
                        Chọn phiên bản Kiosk (Tùy chọn)
                    </CardTitle>
                    <CardDescription>Chọn nếu quy trình cần tương tác với các thiết bị cụ thể theo phiên bản.</CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="space-y-2">
                        <Label htmlFor="kioskVersion" className="flex items-center">
                            Phiên bản Kiosk
                        </Label>
                        <Select
                            value={selectedKioskVersion}
                            onValueChange={setSelectedKioskVersion}
                            disabled={loadingKioskVersions && kioskVersions.length === 0 && !kioskVersionError}
                        >
                            <SelectTrigger id="kioskVersion">
                                <SelectValue
                                    placeholder={
                                        loadingKioskVersions && kioskVersions.length === 0 && !kioskVersionError
                                            ? "Đang tải..."
                                            : "Chọn phiên bản kiosk (nếu cần)"
                                    }
                                />
                            </SelectTrigger>
                            <SelectContent id="kiosk-version-scroll-content" className="max-h-[300px]">
                                {kioskVersionError ? (
                                    <div className="p-4 text-center text-red-500">
                                        <p className="text-sm">{kioskVersionError}</p>
                                        <Button variant="outline" size="sm" className="mt-2" onClick={() => fetchKioskVersions(1)}>
                                            Thử lại
                                        </Button>
                                    </div>
                                ) : loadingKioskVersions && kioskVersions.length === 0 ? (
                                    <div className="p-4 text-center">
                                        <div className="flex items-center justify-center space-x-2">
                                            <Loader2 className="h-4 w-4 animate-spin" />
                                            <span className="text-sm">Đang tải...</span>
                                        </div>
                                    </div>
                                ) : !loadingKioskVersions && kioskVersions.length === 0 ? (
                                    <div className="p-4 text-center text-gray-500">
                                        <p className="text-sm">Chưa có phiên bản kiosk nào</p>
                                    </div>
                                ) : (
                                    <ScrollArea className="h-[200px]">
                                        <InfiniteScroll
                                            dataLength={kioskVersions.length}
                                            next={loadMoreKioskVersions}
                                            hasMore={hasMoreKioskVersion && !loadingKioskVersions}
                                            loader={
                                                <div className="p-2 text-center text-sm flex items-center justify-center space-x-2">
                                                    <Loader2 className="h-3 w-3 animate-spin" />
                                                    <span>Đang tải thêm...</span>
                                                </div>
                                            }
                                            scrollableTarget="kiosk-version-scroll-content"
                                        >
                                            {kioskVersions.map((version) => (
                                                <SelectItem key={version.kioskVersionId} value={version.kioskVersionId}>
                                                    {version.versionTitle}
                                                </SelectItem>
                                            ))}
                                        </InfiniteScroll>
                                    </ScrollArea>
                                )}
                            </SelectContent>
                        </Select>
                        {selectedKioskVersion && kioskVersions.find((v) => v.kioskVersionId === selectedKioskVersion) && (
                            <div className="mt-2">
                                <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                                    Đã chọn: {kioskVersions.find((v) => v.kioskVersionId === selectedKioskVersion)?.versionTitle}
                                </Badge>
                            </div>
                        )}
                        {errors.kioskVersionId && (
                            <p className="text-red-500 text-sm flex items-center">
                                <AlertTriangle className="h-3 w-3 mr-1" />
                                {errors.kioskVersionId[0]}
                            </p>
                        )}
                    </div>
                </CardContent>
            </Card>

            <div className="w-full">
                <Card className="mb-6">
                    <CardHeader>
                        <div className="flex items-center justify-between">
                            <div>
                                <CardTitle>Sơ đồ quy trình</CardTitle>
                                <CardDescription>Click vào bước để sửa, hoặc kéo thả tay cầm để nối các bước.</CardDescription>
                            </div>
                            <div className="flex items-center space-x-2">
                                <Dialog open={showWorkflowInfo} onOpenChange={setShowWorkflowInfo}>
                                    <DialogTrigger asChild>
                                        <Button variant="outline" className="w-full">
                                            <Info className="mr-2 h-4 w-4" />
                                            Xem thông tin quy trình
                                        </Button>
                                    </DialogTrigger>
                                </Dialog>

                                <Button type="button" onClick={addStep} disabled={loading} className="bg-primary hover:bg-primary-200">
                                    <PlusCircle className="mr-2 h-4 w-4" />
                                    Thêm bước
                                </Button>
                            </div>
                        </div>
                    </CardHeader>
                    <CardContent className="h-[600px] p-0">
                        <ReactFlow
                            nodes={nodes}
                            edges={edges}
                            onNodesChange={onNodesChange}
                            onEdgesChange={onEdgesChange}
                            onConnect={onConnect}
                            nodeTypes={nodeTypes}
                            // fitView
                            defaultViewport={{ zoom: 0.75, x: 400, y: 50 }}
                            className="bg-gray-50"
                        >
                            <Controls />
                            <Background variant={BackgroundVariant.Dots} gap={12} size={1} />

                        </ReactFlow>
                    </CardContent>
                </Card>
            </div>

            {/* Workflow Info Dialog */}
            <Dialog open={showWorkflowInfo} onOpenChange={setShowWorkflowInfo}>
                <DialogContent className="sm:max-w-[650px] p-7 border-0 bg-white backdrop-blur-xl shadow-2xl max-h-[90vh] overflow-y-auto hide-scrollbar">
                    <DialogHeader>
                        <DialogTitle>Thông tin quy trình</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4">
                        <div className="space-y-2">
                            <Label htmlFor="name" className="flex items-center">
                                Tên quy trình
                                <span className="text-red-500 ml-1">*</span>
                            </Label>
                            <Input
                                id="name"
                                placeholder="Nhập tên quy trình"
                                value={formData.name}
                                onChange={(e) => handleChange("name", e.target.value)}
                                disabled={loading}
                                className={errors.name ? "border-red-500 focus-visible:ring-red-500" : ""}
                            />
                            {errors.name && (
                                <p className="text-red-500 text-sm flex items-center">
                                    <AlertTriangle className="h-3 w-3 mr-1" />
                                    {errors.name[0]}
                                </p>
                            )}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="type" className="flex items-center">
                                Loại quy trình
                                <span className="text-red-500 ml-1">*</span>
                            </Label>
                            <Select value={formData.type} onValueChange={(value) => handleChange("type", value)} disabled={loading}>
                                <SelectTrigger id="type" className={errors.type ? "border-red-500 focus-visible:ring-red-500" : ""}>
                                    <SelectValue placeholder="Chọn loại quy trình" />
                                </SelectTrigger>
                                <SelectContent>
                                    {Object.values(EWorkflowType).map((type) => (
                                        <SelectItem key={type} value={type}>
                                            {EWorkflowTypeViMap[type]}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            {errors.type && (
                                <p className="text-red-500 text-sm flex items-center">
                                    <AlertTriangle className="h-3 w-3 mr-1" />
                                    {errors.type[0]}
                                </p>
                            )}
                        </div>

                        {formData.type === EWorkflowType.Activity && (
                            <div className="space-y-2">
                                <Label htmlFor="productId" className="flex items-center">
                                    Sản phẩm (Tùy chọn)
                                </Label>
                                <Select
                                    value={formData.productId || ""}
                                    onValueChange={(value) => handleChange("productId", value || null)}
                                    disabled={loading || (loadingProducts && products.length === 0)}
                                >
                                    <SelectTrigger id="productId" className={errors.productId ? "border-red-500 focus-visible:ring-red-500" : ""}>
                                        <SelectValue
                                            placeholder={loadingProducts && products.length === 0 ? "Đang tải sản phẩm..." : "Chọn sản phẩm"}
                                        />
                                    </SelectTrigger>
                                    <SelectContent id="product-scroll-content" className="max-h-[300px]">
                                        <ScrollArea id="product-scroll-area" className="h-[200px]">
                                            <InfiniteScroll
                                                dataLength={products.length}
                                                next={loadMoreProducts}
                                                hasMore={hasMoreProducts && !loadingProducts}
                                                loader={<div className="p-2 text-center text-sm">Đang tải thêm...</div>}
                                                scrollableTarget="product-scroll-area"
                                            >
                                                {products.map((product) => (
                                                    <SelectItem key={product.productId} value={product.productId}>
                                                        {product.name}
                                                    </SelectItem>
                                                ))}
                                                {!loadingProducts && products.length === 0 && (
                                                    <div className="p-2 text-center text-sm text-gray-500">Chưa có sản phẩm.</div>
                                                )}
                                            </InfiniteScroll>
                                        </ScrollArea>
                                    </SelectContent>
                                </Select>
                                {errors.productId && (
                                    <p className="text-red-500 text-sm flex items-center">
                                        <AlertTriangle className="h-3 w-3 mr-1" />
                                        {errors.productId[0]}
                                    </p>
                                )}
                            </div>
                        )}

                        <div className="space-y-2">
                            <Label htmlFor="description" className="flex items-center">
                                Mô tả (Tùy chọn)
                                <TooltipProvider>
                                    <Tooltip>
                                        <TooltipTrigger asChild>
                                            <Info className="h-3.5 w-3.5 ml-1 text-gray-400" />
                                        </TooltipTrigger>
                                        <TooltipContent>
                                            <p>Mô tả chi tiết về quy trình này</p>
                                        </TooltipContent>
                                    </Tooltip>
                                </TooltipProvider>
                            </Label>
                            <Textarea
                                id="description"
                                placeholder="Nhập mô tả quy trình"
                                value={formData.description}
                                onChange={(e) => handleChange("description", e.target.value)}
                                disabled={loading}
                                className="min-h-[120px]"
                            />
                            {errors.description && (
                                <p className="text-red-500 text-sm flex items-center">
                                    <AlertTriangle className="h-3 w-3 mr-1" />
                                    {errors.description[0]}
                                </p>
                            )}
                        </div>
                    </div>
                </DialogContent>
            </Dialog>

            {/* Confirmation Dialog */}
            <Dialog open={isConfirming} onOpenChange={setIsConfirming}>
                <DialogContent className="sm:max-w-[850px] p-0 border-0 bg-white backdrop-blur-xl shadow-2xl max-h-[90vh] flex flex-col">
                    <DialogHeader className="p-6 pb-0">
                        <DialogTitle className="text-xl">Xem lại và Xác nhận Quy trình</DialogTitle>
                        <CardDescription>
                            Vui lòng kiểm tra lại các bước trước khi tạo. Bạn có thể nhấp vào bánh răng để chỉnh sửa lần cuối.
                        </CardDescription>
                    </DialogHeader>
                    <div className="flex-grow overflow-y-auto px-6">
                        {errors.steps && typeof errors.steps === "string" && (
                            <p className="text-red-500 text-sm flex items-center mb-2">
                                <AlertTriangle className="h-3 w-3 mr-1" />
                                {errors.steps}
                            </p>
                        )}
                        {formData.steps.length === 0 ? (
                            <div className="text-center py-12 border-2 border-dashed rounded-lg">
                                <Settings className="h-12 w-12 mx-auto text-gray-400 mb-3" />
                                <p className="text-gray-500">Quy trình phải có ít nhất một bước.</p>
                                <Button type="button" onClick={addStep} variant="outline" className="mt-4">
                                    <PlusCircle className="mr-2 h-4 w-4" />
                                    Thêm bước đầu tiên
                                </Button>
                            </div>
                        ) : (
                            <div className="space-y-3">
                                {formData.steps.map((step, index) => (
                                    <div
                                        key={step.stepCode}
                                        className={`border rounded-md p-4 flex items-center justify-between bg-white dark:bg-gray-800 ${errors.steps?.[index] ? "border-red-500" : "border-gray-200 dark:border-gray-700"}`}
                                    >
                                        <div className="flex items-center flex-grow min-w-0">
                                            {!Number.isNaN(step.sequence) && (
                                                <Badge
                                                    variant="outline"
                                                    className="mr-3 bg-blue-50 text-blue-700 border-blue-200"
                                                >
                                                    {step.sequence}
                                                </Badge>
                                            )}


                                            <span
                                                className="font-medium truncate"
                                                title={step.name || `Bước ${step.sequence}`}
                                            >
                                                {step.name || `Bước ${step.sequence}`}
                                            </span>
                                            {errors.steps?.[index] && (
                                                <AlertTriangle className="h-4 w-4 text-red-500 ml-2 flex-shrink-0" />
                                            )}
                                        </div>
                                        <div className="flex items-center space-x-1 flex-shrink-0 ml-4">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-7 w-7"
                                                onClick={() => setEditingStepIndex(index)}
                                            >
                                                <Settings className="h-4 w-4" />
                                            </Button>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-7 w-7 text-red-500 hover:text-red-600"
                                                onClick={() => removeStep(index)}
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    <DialogFooter className="p-6 pt-4 border-t bg-gray-50 rounded-b-lg">
                        <Button variant="outline" onClick={() => setIsConfirming(false)} disabled={loading}>
                            Quay lại
                        </Button>
                        <Button onClick={handleCreateWorkflow} disabled={loading} className="bg-primary hover:bg-primary-200">
                            {loading ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Đang tạo...
                                </>
                            ) : (
                                <>
                                    <Save className="mr-2 h-4 w-4" />
                                    Xác nhận & Tạo
                                </>
                            )}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            {/* Step Editing Dialog */}
            <Dialog open={editingStepIndex !== null} onOpenChange={(open) => !open && setEditingStepIndex(null)}>
                <DialogContent className="sm:max-w-[650px] p-7 border-0 bg-white backdrop-blur-xl shadow-2xl max-h-[90vh] overflow-y-auto hide-scrollbar">
                    <DialogHeader>
                        <DialogTitle>
                            Chỉnh sửa bước{" "}
                            {Number.isNaN(formData.steps[editingStepIndex ?? 0]?.sequence)
                                ? ""
                                : formData.steps[editingStepIndex ?? 0]?.sequence}
                        </DialogTitle>
                    </DialogHeader>
                    {editingStepIndex !== null && (
                        <div className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <Label htmlFor={`step-sequence-${editingStepIndex}`}>Thứ tự</Label>
                                    <Input
                                        id={`step-sequence-${editingStepIndex}`}
                                        type="number"
                                        value={formData.steps[editingStepIndex].sequence}
                                        onChange={(e) => handleStepChange(editingStepIndex, "sequence", Number.parseInt(e.target.value))}
                                        disabled={loading}
                                        className={errors.steps?.[editingStepIndex]?.sequence ? "border-red-500" : ""}
                                    />
                                    {errors.steps?.[editingStepIndex]?.sequence && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].sequence[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor={`step-name-${editingStepIndex}`}>Tên bước</Label>
                                    <Input
                                        id={`step-name-${editingStepIndex}`}
                                        value={formData.steps[editingStepIndex].name}
                                        onChange={(e) => handleStepChange(editingStepIndex, "name", e.target.value)}
                                        disabled={loading}
                                        className={errors.steps?.[editingStepIndex]?.name ? "border-red-500" : ""}
                                        placeholder="Nhập tên bước"
                                    />
                                    {errors.steps?.[editingStepIndex]?.name && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].name[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label className="asterisk" htmlFor={`step-maxRetries-${editingStepIndex}`}>Số lần thử lại tối đa</Label>
                                    <Input
                                        id={`step-maxRetries-${editingStepIndex}`}
                                        type="number"
                                        value={formData.steps[editingStepIndex].maxRetries}
                                        onChange={(e) => handleStepChange(editingStepIndex, "maxRetries", e.target.value)}
                                        className={errors.steps?.[editingStepIndex]?.maxRetries ? "border-red-500" : ""}
                                    />
                                    {/* Tổng hợp lỗi step nếu có (mảng) */}
                                    {Array.isArray(errors.steps?.[editingStepIndex]) && (
                                        <div className="space-y-1 mb-2">
                                            {errors.steps[editingStepIndex].map((err: string, i: number) => (
                                                <p key={i} className="text-red-500 text-sm">{err}</p>
                                            ))}
                                        </div>
                                    )}

                                    {/* Lỗi từng field nếu có (object) */}
                                    {errors.steps?.[editingStepIndex]?.maxRetries && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].maxRetries[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label className="asterisk" htmlFor={`step-deviceModelId-${editingStepIndex}`}>Mẫu thiết bị</Label>
                                    <Select
                                        value={formData.steps[editingStepIndex].deviceModelId}
                                        onValueChange={(value) => handleStepChange(editingStepIndex, "deviceModelId", value)}
                                        disabled={loading || !selectedKioskVersion || (loadingDeviceModels && deviceModels.length === 0)}
                                    >
                                        <SelectTrigger
                                            id={`step-deviceModelId-${editingStepIndex}`}
                                            className={errors.steps?.[editingStepIndex]?.deviceModelId ? "border-red-500" : ""}
                                        >
                                            <SelectValue
                                                placeholder={
                                                    !selectedKioskVersion
                                                        ? "Chọn phiên bản kiosk trước"
                                                        : loadingDeviceModels && deviceModels.length === 0
                                                            ? "Đang tải mẫu thiết bị..."
                                                            : "Chọn mẫu thiết bị"
                                                }
                                            />
                                        </SelectTrigger>
                                        <SelectContent id={`device-model-scroll-content-${editingStepIndex}`} className="max-h-[300px]">
                                            {selectedKioskVersion && (
                                                <ScrollArea id={`device-model-scroll-area-${editingStepIndex}`} className="h-[200px]">
                                                    <InfiniteScroll
                                                        dataLength={deviceModels.length}
                                                        next={() => fetchDeviceModels(deviceModelPage + 1)}
                                                        hasMore={hasMoreDeviceModels && !loadingDeviceModels}
                                                        loader={<div className="p-2 text-center text-sm">Đang tải thêm...</div>}
                                                        scrollableTarget={`device-model-scroll-area-${editingStepIndex}`}
                                                    >
                                                        {deviceModels.map((deviceModel) => (
                                                            <SelectItem key={deviceModel.deviceModelId} value={deviceModel.deviceModelId}>
                                                                {deviceModel.modelName}
                                                            </SelectItem>
                                                        ))}
                                                        {!loadingDeviceModels && deviceModels.length === 0 && (
                                                            <div className="p-2 text-center text-sm text-gray-500">Chưa có mẫu thiết bị.</div>
                                                        )}
                                                    </InfiniteScroll>
                                                </ScrollArea>
                                            )}
                                        </SelectContent>
                                    </Select>
                                    {errors.steps?.[editingStepIndex]?.deviceModelId && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].deviceModelId[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label className="asterisk" htmlFor={`step-deviceFunctionId-${editingStepIndex}`}>Chức năng thiết bị</Label>
                                    <Select
                                        value={formData.steps[editingStepIndex].deviceFunctionId}
                                        onValueChange={(value) => handleStepChange(editingStepIndex, "deviceFunctionId", value)}
                                        disabled={
                                            loading ||
                                            !formData.steps[editingStepIndex].deviceModelId ||
                                            getDeviceFunctionsForModel(formData.steps[editingStepIndex].deviceModelId).length === 0
                                        }
                                    >
                                        <SelectTrigger
                                            id={`step-deviceFunctionId-${editingStepIndex}`}
                                            className={errors.steps?.[editingStepIndex]?.deviceFunctionId ? "border-red-500" : ""}
                                        >
                                            <SelectValue
                                                placeholder={
                                                    !formData.steps[editingStepIndex].deviceModelId
                                                        ? "Chọn mẫu thiết bị trước"
                                                        : getDeviceFunctionsForModel(formData.steps[editingStepIndex].deviceModelId).length === 0
                                                            ? "Chưa có chức năng"
                                                            : "Chọn chức năng"
                                                }
                                            />
                                        </SelectTrigger>
                                        <SelectContent className="max-h-[300px]">
                                            {getDeviceFunctionsForModel(formData.steps[editingStepIndex].deviceModelId).map((df) => (
                                                <SelectItem key={df.deviceFunctionId || df.name} value={df.deviceFunctionId || df.name}>
                                                    <div className="flex flex-col">
                                                        <span className="font-medium">{df.label}</span>
                                                        {df.functionParameters && df.functionParameters.length > 0 && (
                                                            <span className="text-xs text-muted-foreground">{df.functionParameters.length} tham số</span>
                                                        )}
                                                    </div>
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                    {errors.steps?.[editingStepIndex]?.deviceFunctionId && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].deviceFunctionId[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor={`step-callbackStepCode-${editingStepIndex}`}>Bước gọi lại (Tùy chọn)</Label>
                                    <Select
                                        value={formData.steps[editingStepIndex].callbackStepCode ?? ""}
                                        onValueChange={(value) => handleStepChange(editingStepIndex, "callbackStepCode", value)}
                                        disabled={loading}
                                    >
                                        <SelectTrigger
                                            id={`step-callbackStepCode-${editingStepIndex}`}
                                            className={errors.steps?.[editingStepIndex]?.callbackStepCode ? "border-red-500" : ""}
                                        >
                                            <SelectValue placeholder="Chọn bước gọi lại trong quy trình này" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            {formData.steps
                                                .filter((_, idx) => idx !== editingStepIndex)
                                                .map((s) => (
                                                    <SelectItem key={s.stepCode} value={s.stepCode}>
                                                        {s.name || `Bước ${s.sequence}`} (Sequence: {s.sequence})
                                                    </SelectItem>
                                                ))}
                                        </SelectContent>
                                    </Select>
                                    {errors.steps?.[editingStepIndex]?.callbackStepCode && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].callbackStepCode[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor={`step-system-${editingStepIndex}`} className="flex items-center">
                                        Hệ thống
                                        <span className="text-red-500 ml-1">*</span>
                                    </Label>
                                    <Select
                                        value={formData.steps[editingStepIndex].system || "arm"}
                                        onValueChange={(value) => handleStepChange(editingStepIndex, "system", value)}
                                        disabled={loading}
                                    >
                                        <SelectTrigger
                                            id={`step-system-${editingStepIndex}`}
                                            className={errors.steps?.[editingStepIndex]?.system ? "border-red-500" : ""}
                                        >
                                            <SelectValue placeholder="Chọn hệ thống" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="arm">Robot Arm (Cánh tay robot)</SelectItem>
                                            <SelectItem value="iot">IoT Device (Thiết bị IoT)</SelectItem>
                                        </SelectContent>
                                    </Select>
                                    {errors.steps?.[editingStepIndex]?.system && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].system[0]}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor={`step-timeout-${editingStepIndex}`} className="flex items-center">
                                        Timeout (giây)
                                        <span className="text-red-500 ml-1">*</span>
                                    </Label>
                                    <Input
                                        id={`step-timeout-${editingStepIndex}`}
                                        type="number"
                                        min="1"
                                        max="300"
                                        value={formData.steps[editingStepIndex].timeout || 20}
                                        onChange={(e) => handleStepChange(editingStepIndex, "timeout", Number.parseInt(e.target.value) || 20)}
                                        disabled={loading}
                                        className={errors.steps?.[editingStepIndex]?.timeout ? "border-red-500" : ""}
                                        placeholder="20"
                                    />
                                    {errors.steps?.[editingStepIndex]?.timeout && (
                                        <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].timeout[0]}</p>
                                    )}
                                </div>

                                {/* Request Configuration based on System */}
                                {formData.steps[editingStepIndex].system === "arm" ? (
                                    // ARM: Lua Script Selection
                                    <div className="space-y-2 md:col-span-2">
                                        <Label htmlFor={`step-luaFile-${editingStepIndex}`} className="flex items-center">
                                            File Lua Script
                                            <span className="text-red-500 ml-1">*</span>
                                        </Label>
                                        <Select
                                            value={formData.steps[editingStepIndex].luaFile || ""}
                                            onValueChange={(value) => handleStepChange(editingStepIndex, "luaFile", value)}
                                            disabled={loading || loadingLuaScripts}
                                        >
                                            <SelectTrigger
                                                id={`step-luaFile-${editingStepIndex}`}
                                                className={errors.steps?.[editingStepIndex]?.luaFile ? "border-red-500" : ""}
                                            >
                                                <SelectValue
                                                    placeholder={
                                                        loadingLuaScripts
                                                            ? "Đang tải danh sách Lua scripts..."
                                                            : "Chọn file Lua script"
                                                    }
                                                />
                                            </SelectTrigger>
                                            <SelectContent className="max-h-[300px]">
                                                {luaScripts.length === 0 ? (
                                                    <div className="p-2 text-center text-sm text-gray-500">
                                                        {loadingLuaScripts ? "Đang tải..." : "Chưa có Lua script nào"}
                                                    </div>
                                                ) : (
                                                    luaScripts.map((script) => (
                                                        <SelectItem
                                                            key={script.scriptId}
                                                            value={script.fileName || script.name}
                                                        >
                                                            <div className="flex flex-col">
                                                                <span className="font-medium">{script.fileName || script.name}</span>
                                                                {script.description && (
                                                                    <span className="text-xs text-muted-foreground">{script.description}</span>
                                                                )}
                                                            </div>
                                                        </SelectItem>
                                                    ))
                                                )}
                                            </SelectContent>
                                        </Select>
                                        {errors.steps?.[editingStepIndex]?.luaFile && (
                                            <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].luaFile[0]}</p>
                                        )}
                                        <p className="text-xs text-muted-foreground">
                                            Chọn file Lua script để chạy trên cánh tay robot. File sẽ được thực thi khi bước này được thực hiện.
                                        </p>
                                    </div>
                                ) : (
                                    // IOT: Hex Code Input
                                    <div className="space-y-4 md:col-span-2">
                                        <div className="space-y-2">
                                            <Label htmlFor={`step-iotDevice-${editingStepIndex}`} className="flex items-center">
                                                Tên thiết bị (Device Name)
                                                <span className="text-red-500 ml-1">*</span>
                                            </Label>
                                            <Input
                                                id={`step-iotDevice-${editingStepIndex}`}
                                                value={formData.steps[editingStepIndex].iotDevice || ""}
                                                onChange={(e) => handleStepChange(editingStepIndex, "iotDevice", e.target.value)}
                                                disabled={loading}
                                                placeholder="Ví dụ: IceMake, Pump, Scale"
                                                className={errors.steps?.[editingStepIndex]?.iotDevice ? "border-red-500" : ""}
                                            />
                                            {errors.steps?.[editingStepIndex]?.iotDevice && (
                                                <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].iotDevice[0]}</p>
                                            )}
                                            <p className="text-xs text-muted-foreground">
                                                Tên thiết bị IoT (phải khớp với tên trong devices.json của IotController)
                                            </p>
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor={`step-iotCommand-${editingStepIndex}`} className="flex items-center">
                                                Lệnh (Command)
                                                <span className="text-red-500 ml-1">*</span>
                                            </Label>
                                            <Select
                                                value={formData.steps[editingStepIndex].iotCommand || "send_hex"}
                                                onValueChange={(value) => handleStepChange(editingStepIndex, "iotCommand", value)}
                                                disabled={loading}
                                            >
                                                <SelectTrigger
                                                    id={`step-iotCommand-${editingStepIndex}`}
                                                    className={errors.steps?.[editingStepIndex]?.iotCommand ? "border-red-500" : ""}
                                                >
                                                    <SelectValue placeholder="Chọn lệnh" />
                                                </SelectTrigger>
                                                <SelectContent>
                                                    <SelectItem value="send_hex">Send Hex (Gửi mã hex)</SelectItem>
                                                    <SelectItem value="read">Read (Đọc dữ liệu)</SelectItem>
                                                </SelectContent>
                                            </Select>
                                            {errors.steps?.[editingStepIndex]?.iotCommand && (
                                                <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].iotCommand[0]}</p>
                                            )}
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor={`step-iotHex-${editingStepIndex}`} className="flex items-center">
                                                Mã Hex
                                                <span className="text-red-500 ml-1">*</span>
                                            </Label>
                                            <Input
                                                id={`step-iotHex-${editingStepIndex}`}
                                                value={formData.steps[editingStepIndex].iotHex || ""}
                                                onChange={(e) => handleStepChange(editingStepIndex, "iotHex", e.target.value)}
                                                disabled={loading}
                                                placeholder="Ví dụ: 04 07 AA 02 05 BC FF"
                                                className={errors.steps?.[editingStepIndex]?.iotHex ? "border-red-500" : ""}
                                            />
                                            {errors.steps?.[editingStepIndex]?.iotHex && (
                                                <p className="text-red-500 text-sm">{errors.steps[editingStepIndex].iotHex[0]}</p>
                                            )}
                                            <p className="text-xs text-muted-foreground">
                                                Nhập mã hex để điều khiển thiết bị IoT (các byte cách nhau bằng khoảng trắng).
                                                {formData.steps[editingStepIndex].deviceFunctionId && formData.steps[editingStepIndex].iotHex && (
                                                    <span className="text-green-600 font-medium ml-1">
                                                        (Đã tự động điền từ chức năng đã chọn)
                                                    </span>
                                                )}
                                            </p>
                                        </div>

                                        <div className="grid grid-cols-2 gap-4">
                                            <div className="space-y-2">
                                                <Label htmlFor={`step-iotFlush-${editingStepIndex}`} className="flex items-center">
                                                    Flush
                                                </Label>
                                                <Select
                                                    value={formData.steps[editingStepIndex].iotFlush ? "true" : "false"}
                                                    onValueChange={(value) => handleStepChange(editingStepIndex, "iotFlush", value === "true")}
                                                    disabled={loading}
                                                >
                                                    <SelectTrigger id={`step-iotFlush-${editingStepIndex}`}>
                                                        <SelectValue />
                                                    </SelectTrigger>
                                                    <SelectContent>
                                                        <SelectItem value="true">Có (true)</SelectItem>
                                                        <SelectItem value="false">Không (false)</SelectItem>
                                                    </SelectContent>
                                                </Select>
                                            </div>

                                            <div className="space-y-2">
                                                <Label htmlFor={`step-iotReadLen-${editingStepIndex}`} className="flex items-center">
                                                    Độ dài đọc (Read Length)
                                                </Label>
                                                <Input
                                                    id={`step-iotReadLen-${editingStepIndex}`}
                                                    type="number"
                                                    min="0"
                                                    max="256"
                                                    value={formData.steps[editingStepIndex].iotReadLen || 16}
                                                    onChange={(e) => handleStepChange(editingStepIndex, "iotReadLen", Number.parseInt(e.target.value) || 16)}
                                                    disabled={loading}
                                                    placeholder="16"
                                                />
                                            </div>
                                        </div>
                                    </div>
                                )}
                            </div>

                            <div className="space-y-4">
                                <div className="flex items-center justify-between">
                                    <Label>Điều kiện</Label>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => addCondition(editingStepIndex)}
                                        disabled={loading}
                                    >
                                        <PlusCircle className="h-4 w-4 mr-2" />
                                        Thêm điều kiện
                                    </Button>
                                </div>
                                {formData.steps[editingStepIndex].conditions?.length === 0 ? (
                                    <p className="text-gray-500 text-sm">Chưa có điều kiện nào được thêm.</p>
                                ) : (
                                    <div className="space-y-4">
                                        {formData.steps[editingStepIndex].conditions?.map((condition, conditionIndex) => (
                                            <div key={conditionIndex} className="border p-4 rounded-md space-y-4">
                                                <div className="flex items-center justify-between">
                                                    <h4 className="font-medium">Điều kiện {conditionIndex + 1}</h4>
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => removeCondition(editingStepIndex, conditionIndex)}
                                                        disabled={loading}
                                                    >
                                                        <Trash2 className="h-4 w-4 text-red-500" />
                                                    </Button>
                                                </div>

                                                <div className="space-y-2">
                                                    <Label>Tên điều kiện</Label>
                                                    <Select
                                                        value={condition.name}
                                                        onValueChange={(value) => handleConditionChange(editingStepIndex, conditionIndex, "name", value)}
                                                        disabled={loading}
                                                    >
                                                        <SelectTrigger>
                                                            <SelectValue placeholder="Chọn tên điều kiện" />
                                                        </SelectTrigger>
                                                        <SelectContent>
                                                            {Object.values(EConditionName).map((name) => (
                                                                <SelectItem key={name} value={name}>
                                                                    {EConditionNameViMap[name]}
                                                                </SelectItem>
                                                            ))}
                                                        </SelectContent>
                                                    </Select>
                                                </div>

                                                <div className="space-y-2">
                                                    <Label>Mô tả</Label>
                                                    <Input
                                                        value={condition.description || ""}
                                                        onChange={(e) => handleConditionChange(editingStepIndex, conditionIndex, "description", e.target.value)}
                                                        disabled={loading}
                                                        placeholder="Nhập mô tả điều kiện"
                                                    />
                                                </div>

                                                <div className="space-y-2">
                                                    <Label>Biểu thức (Expression)</Label>
                                                    <div className="p-2 mt-2 border rounded bg-gray-50 text-sm">
                                                        <strong>Xem trước:</strong>{" "}
                                                        {condition.expression.left.value}{" "}
                                                        {EOperationMap[condition.expression.operator]}{" "}
                                                        {condition.expression.right.value}
                                                    </div>
                                                    <div className="grid grid-cols-1 gap-4">
                                                        <div className="space-y-2">
                                                            <Label>Phần trái</Label>
                                                            <Select
                                                                value={condition.expression.left.type}
                                                                onValueChange={(value) => handleConditionChange(editingStepIndex, conditionIndex, "leftType", value)}
                                                                disabled={loading}
                                                            >
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Chọn loại" />
                                                                </SelectTrigger>
                                                                <SelectContent>
                                                                    {Object.values(EExpressionType).map((type) => (
                                                                        <SelectItem key={type} value={type}>
                                                                            {EExpressionTypeViMap[type]}
                                                                        </SelectItem>
                                                                    ))}
                                                                </SelectContent>
                                                            </Select>
                                                            <Input
                                                                value={condition.expression.left.value}
                                                                onChange={(e) => handleConditionChange(editingStepIndex, conditionIndex, "leftValue", e.target.value)}
                                                                disabled={loading}
                                                                placeholder={condition.expression.left.type === EExpressionType.Variable ? "Tên biến" : "Giá trị"}
                                                            />
                                                        </div>

                                                        <div className="space-y-2">
                                                            <Label>Toán tử</Label>
                                                            <Select
                                                                value={condition.expression.operator}
                                                                onValueChange={(value) => handleConditionChange(editingStepIndex, conditionIndex, "operator", value)}
                                                                disabled={loading}
                                                            >
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Chọn toán tử" />
                                                                </SelectTrigger>
                                                                <SelectContent>
                                                                    {Object.values(EOperation).map((op) => (
                                                                        <SelectItem key={op} value={op}>
                                                                            {EOperationViMap[op]} ({op})
                                                                        </SelectItem>
                                                                    ))}
                                                                </SelectContent>
                                                            </Select>
                                                        </div>

                                                        <div className="space-y-2">
                                                            <Label>Phần phải</Label>
                                                            <Select
                                                                value={condition.expression.right.type}
                                                                onValueChange={(value) => handleConditionChange(editingStepIndex, conditionIndex, "rightType", value)}
                                                                disabled={loading}
                                                            >
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Chọn loại" />
                                                                </SelectTrigger>
                                                                <SelectContent>
                                                                    {Object.values(EExpressionType).map((type) => (
                                                                        <SelectItem key={type} value={type}>
                                                                            {EExpressionTypeViMap[type]}
                                                                        </SelectItem>
                                                                    ))}
                                                                </SelectContent>
                                                            </Select>
                                                            <Input
                                                                value={condition.expression.right.value}
                                                                onChange={(e) => handleConditionChange(editingStepIndex, conditionIndex, "rightValue", e.target.value)}
                                                                disabled={loading}
                                                                placeholder={condition.expression.right.type === EExpressionType.Variable ? "Tên biến" : "Giá trị"}
                                                            />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    )}
                </DialogContent>
            </Dialog>

        </div>
    )
}

export default CreateWorkflow