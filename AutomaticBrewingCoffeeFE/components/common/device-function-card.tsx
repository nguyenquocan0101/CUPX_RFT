"use client"

import { useState, useRef } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible"
import { Trash2, Plus, ChevronDown, ChevronRight, Settings, Upload, FileCode } from 'lucide-react'
import { DeviceFunction } from "@/interfaces/device"
import { EBaseStatusViMap } from "@/enum/base"
import { EFunctionParameterType, EFunctionParameterTypeViMap } from "@/enum/device"
import { DynamicOptionsInput } from "./dynamic-option-input"
import { LuaScript } from "@/services/luaScript.service"
import { useToast } from "@/hooks/use-toast"
import { uploadLuaScript, getLuaScripts } from "@/services/luaScript.service"

interface DeviceFunctionCardProps {
    func: DeviceFunction
    index: number
    onUpdate: (index: number, field: string, value: any) => void
    onRemove: (index: number) => void
    onAddParameter: (functionIndex: number, initialData?: Partial<{
        name: string;
        type: EFunctionParameterType;
        default: string;
        description: string | null;
    }>) => void
    onRemoveParameter: (functionIndex: number, paramIndex: number) => void
    onUpdateParameter: (functionIndex: number, paramIndex: number, field: string, value: any) => void
    errors?: Record<string, any>
    luaScripts?: LuaScript[]
    onLuaScriptsRefresh?: () => void
}

export function DeviceFunctionCard({
    func,
    index,
    onUpdate,
    onRemove,
    onAddParameter,
    onRemoveParameter,
    onUpdateParameter,
    errors = {},
    luaScripts = [],
    onLuaScriptsRefresh,
}: DeviceFunctionCardProps) {
    const [isOpen, setIsOpen] = useState(true)
    const [uploading, setUploading] = useState(false)
    const fileInputRef = useRef<HTMLInputElement>(null)
    const { toast } = useToast()

    const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (!file) return

        if (!file.name.endsWith('.lua')) {
            toast({
                title: "Lỗi",
                description: "Chỉ chấp nhận file .lua",
                variant: "destructive",
            })
            return
        }

        setUploading(true)
        try {
            // Thử upload với overwrite=false trước
            let result = await uploadLuaScript(file, false)
            
            // Nếu thành công
            if (result && result.file) {
                toast({
                    title: "Thành công",
                    description: `Đã upload file ${result.file}`,
                })
                // Refresh danh sách Lua scripts
                if (onLuaScriptsRefresh) {
                    onLuaScriptsRefresh()
                }
                // Tự động chọn file vừa upload
                onUpdate(index, "luaScriptFileName", result.file)
                return
            }
            
            throw new Error("Response không hợp lệ từ server")
        } catch (error: any) {
            console.error("Upload error:", error)
            const errorMessage = error?.response?.data?.message || error?.message || error?.error || ""
            
            // Kiểm tra nếu lỗi do file đã tồn tại
            const isFileExistsError = errorMessage.includes("đã tồn tại") || 
                                     errorMessage.includes("already exists") ||
                                     errorMessage.includes("File đã tồn tại")
            
            if (isFileExistsError) {
                // Tự động thử lại với overwrite=true
                try {
                    const result = await uploadLuaScript(file, true)
                    if (result && result.file) {
                        toast({
                            title: "Thành công",
                            description: `Đã ghi đè file ${result.file}`,
                        })
                        // Refresh danh sách Lua scripts
                        if (onLuaScriptsRefresh) {
                            onLuaScriptsRefresh()
                        }
                        // Tự động chọn file vừa upload
                        onUpdate(index, "luaScriptFileName", result.file)
                        return
                    }
                } catch (retryError: any) {
                    const retryErrorMessage = retryError?.response?.data?.message || retryError?.message || retryError?.error || "Không thể upload file"
                    toast({
                        title: "Lỗi upload",
                        description: retryErrorMessage,
                        variant: "destructive",
                    })
                }
            } else {
                // Lỗi khác
                toast({
                    title: "Lỗi upload",
                    description: errorMessage || "Không thể upload file",
                    variant: "destructive",
                })
            }
        } finally {
            setUploading(false)
            // Reset input
            if (fileInputRef.current) {
                fileInputRef.current.value = ""
            }
        }
    }

    return (
        <Card className="border-l-4 border-l-primary">
            <Collapsible open={isOpen} onOpenChange={setIsOpen}>
                <CardHeader className="pb-3">
                    <div className="flex items-center justify-between">
                        <CollapsibleTrigger asChild>
                            <Button variant="ghost" className="p-0 h-auto font-semibold text-left">
                                <div className="flex items-center gap-2">
                                    {isOpen ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                                    <Settings className="h-4 w-4" />
                                    <CardTitle className="text-base">
                                        {func.name || `Chức năng ${index + 1}`}
                                    </CardTitle>
                                </div>
                            </Button>
                        </CollapsibleTrigger>
                        <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            onClick={() => onRemove(index)}
                        >
                            <Trash2 className="h-4 w-4" />
                        </Button>
                    </div>
                </CardHeader>

                <CollapsibleContent>
                    <CardContent className="space-y-4">
                        {/* Function Name */}
                        <div className="space-y-2">
                            <Label htmlFor={`func-name-${index}`}>
                                Tên chức năng
                                <span className="text-red-500 ml-1">*</span>
                            </Label>
                            <Input
                                id={`func-name-${index}`}
                                value={func.name}
                                onChange={(e) => onUpdate(index, "name", e.target.value)}
                                placeholder="Nhập tên chức năng"
                            />
                            {errors.name && <p className="text-red-500 text-xs">{errors.name}</p>}
                        </div>

                        {/* Function Label */}
                        <div className="space-y-2">
                            <Label htmlFor={`func-label-${index}`}>
                                Label
                                <span className="text-red-500 ml-1">*</span>
                            </Label>
                            <Input
                                id={`func-label-${index}`}
                                value={func.label || ""}
                                onChange={(e) => onUpdate(index, "label", e.target.value)}
                                placeholder="Ví dụ: Bật đèn, Tắt máy..."
                            />
                            {errors.label && <p className="text-red-500 text-xs">{errors.label}</p>}
                        </div>

                        {/* Function Status */}
                        <div className="space-y-2">
                            <Label htmlFor={`func-status-${index}`}>Trạng thái</Label>
                            <Select
                                value={func.status}
                                onValueChange={(value) => onUpdate(index, "status", value)}
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Chọn trạng thái" />
                                </SelectTrigger>
                                <SelectContent>
                                    {Object.entries(EBaseStatusViMap).map(([key, value]) => (
                                        <SelectItem key={key} value={key}>
                                            {value}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            {errors.status && <p className="text-red-500 text-xs">{errors.status}</p>}
                        </div>

                        {/* Lua Script File */}
                        <div className="space-y-2">
                            <div className="flex items-center justify-between">
                                <Label htmlFor={`func-lua-${index}`}>
                                    File Lua Script
                                    <span className="text-xs text-gray-500 ml-2">(Tùy chọn - cho cánh tay robot)</span>
                                </Label>
                                <Button
                                    type="button"
                                    variant="outline"
                                    size="sm"
                                    onClick={() => fileInputRef.current?.click()}
                                    disabled={uploading}
                                    className="h-8 text-xs"
                                >
                                    <Upload className="h-3 w-3 mr-1" />
                                    {uploading ? "Đang upload..." : "Upload file"}
                                </Button>
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept=".lua"
                                    onChange={handleFileUpload}
                                    className="hidden"
                                />
                            </div>
                            <div className="grid grid-cols-[1fr_auto] gap-2">
                                <div className="flex flex-col gap-2">
                                    <Select
                                        value={func.luaScriptFileName && luaScripts.find(s => (s.fileName || s.name) === func.luaScriptFileName) ? func.luaScriptFileName : undefined}
                                        onValueChange={(value) => {
                                            onUpdate(index, "luaScriptFileName", value || null)
                                        }}
                                    >
                                        <SelectTrigger>
                                            <SelectValue placeholder="Chọn từ danh sách..." />
                                        </SelectTrigger>
                                        <SelectContent>
                                            {luaScripts.length === 0 ? (
                                                <div className="px-2 py-1.5 text-xs text-gray-500">
                                                    Chưa có file Lua script. Upload file mới bằng nút "Upload file" phía trên.
                                                </div>
                                            ) : (
                                                luaScripts.map((script) => (
                                                    <SelectItem key={script.scriptId} value={script.fileName || script.name}>
                                                        <div className="flex items-center gap-2">
                                                            <FileCode className="h-3 w-3" />
                                                            <span>{script.fileName || script.name}</span>
                                                            {script.description && (
                                                                <span className="text-xs text-gray-500 ml-2">({script.description})</span>
                                                            )}
                                                        </div>
                                                    </SelectItem>
                                                ))
                                            )}
                                        </SelectContent>
                                    </Select>
                                    <Input
                                        value={func.luaScriptFileName || ""}
                                        onChange={(e) => onUpdate(index, "luaScriptFileName", e.target.value || null)}
                                        placeholder="Hoặc nhập tên file thủ công (VD: MoveToMotor.lua)"
                                        className="h-8 text-xs"
                                    />
                                </div>
                            </div>
                            {func.luaScriptFileName && !luaScripts.find(s => (s.fileName || s.name) === func.luaScriptFileName) && (
                                <div className="p-2 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
                                    <p>
                                        ⚠️ File này chưa có trong database. Vui lòng upload file bằng nút "Upload file" phía trên hoặc đảm bảo file đã được sync vào hệ thống.
                                    </p>
                                </div>
                            )}
                            <p className="text-xs text-gray-500">
                                Chọn file Lua script từ danh sách hoặc nhập tên file thủ công. Khi gọi chức năng này trong Workflow, hệ thống sẽ tự động chạy Lua script này.
                            </p>
                            {errors.luaScriptFileName && <p className="text-red-500 text-xs">{errors.luaScriptFileName}</p>}
                        </div>

                        {/* Function Parameters */}
                        <div className="space-y-3">
                            <div className="flex items-center justify-between">
                                <Label className="text-sm font-medium">Tham số chức năng</Label>
                                <div className="flex gap-2">
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={() => {
                                            // Tự động thêm parameter hex cho IoT devices với dữ liệu ban đầu
                                            onAddParameter(index, {
                                                name: "hex",
                                                type: EFunctionParameterType.Text,
                                                default: "",
                                                description: "Mã hex để điều khiển thiết bị IoT (VD: 04 07 AA 02 05 BC FF)"
                                            })
                                        }}
                                        title="Thêm tham số Hex cho thiết bị IoT - Tự động tạo parameter 'hex' với placeholder mã hex"
                                        className="bg-blue-50 hover:bg-blue-100 border-blue-300 text-blue-700"
                                    >
                                        <Plus className="h-4 w-4 mr-1" />
                                        Thêm Hex
                                    </Button>
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={() => onAddParameter(index)}
                                    >
                                        <Plus className="h-4 w-4 mr-1" />
                                        Thêm tham số
                                    </Button>
                                </div>
                            </div>
                            <div className="p-3 bg-blue-50 border border-blue-200 rounded-md text-xs text-blue-800">
                                <p className="font-medium mb-1">💡 Hướng dẫn cho thiết bị IoT:</p>
                                <ul className="list-disc list-inside space-y-1 ml-2">
                                    <li>Để lưu mã hex cho chức năng, thêm tham số với tên <strong>"hex"</strong> hoặc <strong>"hexCode"</strong></li>
                                    <li>Nhập mã hex vào <strong>"Giá trị mặc định"</strong> (VD: <code>04 07 AA 02 05 BC FF</code>)</li>
                                    <li>Khi tạo workflow, mã hex sẽ tự động được điền từ chức năng này</li>
                                </ul>
                            </div>

                            {func.functionParameters.length === 0 ? (
                                <div className="text-center py-4 text-muted-foreground border-2 border-dashed rounded-md">
                                    Chưa có tham số nào. Nhấn "Thêm tham số" để bắt đầu.
                                </div>
                            ) : (
                                <div className="space-y-3">
                                    {func.functionParameters.map((param, paramIndex) => {
                                        const paramErrors = errors.functionParameters?.[paramIndex] || {};
                                        return (
                                            <Card key={paramIndex} className="bg-muted/30">
                                                <CardHeader className="pb-2">
                                                    <div className="flex items-center justify-between">
                                                        <CardTitle className="text-sm">
                                                            {param.name || `Tham số ${paramIndex + 1}`}
                                                        </CardTitle>
                                                        <Button
                                                            type="button"
                                                            variant="destructive"
                                                            size="sm"
                                                            onClick={() => onRemoveParameter(index, paramIndex)}
                                                        >
                                                            <Trash2 className="h-3 w-3" />
                                                        </Button>
                                                    </div>
                                                </CardHeader>
                                                <CardContent className="space-y-3">
                                                    <div className="grid grid-cols-2 gap-3">
                                                        {/* Parameter Name */}
                                                        <div className="space-y-2">
                                                            <Label htmlFor={`param-name-${index}-${paramIndex}`}>
                                                                Tên tham số
                                                                <span className="text-red-500 ml-1">*</span>
                                                            </Label>
                                                            <Input
                                                                id={`param-name-${index}-${paramIndex}`}
                                                                value={param.name}
                                                                onChange={(e) =>
                                                                    onUpdateParameter(index, paramIndex, "name", e.target.value)
                                                                }
                                                                placeholder="Nhập tên tham số"
                                                            />
                                                            {paramErrors.name && (
                                                                <p className="text-red-500 text-xs">{paramErrors.name}</p>
                                                            )}
                                                        </div>

                                                        {/* Parameter Type */}
                                                        <div className="space-y-2">
                                                            <Label htmlFor={`param-type-${index}-${paramIndex}`}>Kiểu tham số</Label>
                                                            <Select
                                                                value={param.type}
                                                                onValueChange={(value) =>
                                                                    onUpdateParameter(index, paramIndex, "type", value)
                                                                }
                                                            >
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Chọn kiểu" />
                                                                </SelectTrigger>
                                                                <SelectContent>
                                                                    {Object.values(EFunctionParameterType).map((type) => (
                                                                        <SelectItem key={type} value={type}>
                                                                            {EFunctionParameterTypeViMap[type]}
                                                                        </SelectItem>
                                                                    ))}
                                                                </SelectContent>
                                                            </Select>
                                                            {paramErrors.type && (
                                                                <p className="text-red-500 text-xs">{paramErrors.type}</p>
                                                            )}
                                                        </div>
                                                    </div>

                                                    {/* Min/Max Values */}
                                                    {(param.type === EFunctionParameterType.Double || param.type === EFunctionParameterType.Integer) && (
                                                        <div className="grid grid-cols-2 gap-3">
                                                            <div className="space-y-2">
                                                                <Label htmlFor={`param-min-${index}-${paramIndex}`}>Giá trị tối thiểu</Label>
                                                                <Input
                                                                    id={`param-min-${index}-${paramIndex}`}
                                                                    value={param.min ?? ""}
                                                                    onChange={(e) =>
                                                                        onUpdateParameter(index, paramIndex, "min", e.target.value)
                                                                    }
                                                                    placeholder="Tùy chọn"
                                                                />
                                                                {paramErrors.min && (
                                                                    <p className="text-red-500 text-xs">{paramErrors.min}</p>
                                                                )}
                                                            </div>
                                                            <div className="space-y-2">
                                                                <Label htmlFor={`param-max-${index}-${paramIndex}`}>Giá trị tối đa</Label>
                                                                <Input
                                                                    id={`param-max-${index}-${paramIndex}`}
                                                                    value={param.max ?? ""}
                                                                    onChange={(e) =>
                                                                        onUpdateParameter(index, paramIndex, "max", e.target.value)
                                                                    }
                                                                    placeholder="Tùy chọn"
                                                                />
                                                                {paramErrors.max && (
                                                                    <p className="text-red-500 text-xs">{paramErrors.max}</p>
                                                                )}
                                                            </div>
                                                        </div>
                                                    )}

                                                    {/* Dynamic Options Input */}
                                                    <DynamicOptionsInput
                                                        label="Tùy chọn"
                                                        value={param.options}
                                                        onChange={(options) => onUpdateParameter(index, paramIndex, "options", options)}
                                                        placeholder="Nhập tùy chọn và nhấn Enter hoặc nút +"
                                                    />

                                                    {/* Default value */}
                                                    <div className="space-y-2">
                                                        <Label>
                                                            Giá trị mặc định
                                                            <span className="text-red-500 ml-1">*</span>
                                                            {(param.name?.toLowerCase().includes("hex") || param.name?.toLowerCase().includes("code")) && (
                                                                <span className="text-xs text-blue-600 ml-2 font-normal">
                                                                    (Cho IoT: nhập mã hex, VD: 04 07 AA 02 05 BC FF)
                                                                </span>
                                                            )}
                                                        </Label>
                                                        {param.type === EFunctionParameterType.Boolean ? (
                                                            <Select
                                                                value={
                                                                    typeof param.default === "string"
                                                                        ? param.default
                                                                        : param.default === true
                                                                            ? "true"
                                                                            : param.default === false
                                                                                ? "false"
                                                                                : ""
                                                                }
                                                                onValueChange={(value) =>
                                                                    onUpdateParameter(index, paramIndex, "default", value === "true")
                                                                }
                                                            >
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Chọn true/false" />
                                                                </SelectTrigger>
                                                                <SelectContent>
                                                                    <SelectItem value="true">True</SelectItem>
                                                                    <SelectItem value="false">False</SelectItem>
                                                                </SelectContent>
                                                            </Select>
                                                        ) : (
                                                            <div className="space-y-1">
                                                                <Input
                                                                    value={param.default || ""}
                                                                    onChange={(e) =>
                                                                        onUpdateParameter(index, paramIndex, "default", e.target.value)
                                                                    }
                                                                    placeholder={
                                                                        (param.name?.toLowerCase().includes("hex") || param.name?.toLowerCase().includes("code"))
                                                                            ? "04 07 AA 02 05 BC FF"
                                                                            : "Nhập giá trị mặc định"
                                                                    }
                                                                    className={
                                                                        (param.name?.toLowerCase().includes("hex") || param.name?.toLowerCase().includes("code"))
                                                                            ? "font-mono text-sm"
                                                                            : ""
                                                                    }
                                                                />
                                                                {(param.name?.toLowerCase().includes("hex") || param.name?.toLowerCase().includes("code")) && (
                                                                    <p className="text-xs text-gray-500">
                                                                        💡 Format: các byte hex cách nhau bằng khoảng trắng (VD: 04 07 AA 02 05 BC FF)
                                                                    </p>
                                                                )}
                                                            </div>
                                                        )}
                                                        {paramErrors.default && (
                                                            <p className="text-red-500 text-xs">{paramErrors.default}</p>
                                                        )}
                                                        {(param.name?.toLowerCase().includes("hex") || param.name?.toLowerCase().includes("code")) && param.default && (
                                                            <p className="text-xs text-green-600">
                                                                ✓ Mã hex này sẽ tự động được sử dụng khi tạo workflow với chức năng này
                                                            </p>
                                                        )}
                                                    </div>

                                                    {/* Description */}
                                                    <div className="space-y-2">
                                                        <Label>Mô tả</Label>
                                                        <Input
                                                            value={param.description ?? ""}
                                                            onChange={(e) =>
                                                                onUpdateParameter(index, paramIndex, "description", e.target.value)
                                                            }
                                                            placeholder="Nhập mô tả (tối đa 450 ký tự)"
                                                            maxLength={450}
                                                        />
                                                        {paramErrors.description && (
                                                            <p className="text-red-500 text-xs">{paramErrors.description}</p>
                                                        )}
                                                    </div>
                                                </CardContent>
                                            </Card>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </CardContent>
                </CollapsibleContent>
            </Collapsible>
        </Card>
    )
}