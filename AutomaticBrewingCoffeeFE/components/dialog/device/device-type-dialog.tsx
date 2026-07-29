"use client";

import type React from "react";
import { useState, useEffect, useRef } from "react";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { MapPin, CheckCircle2, AlertCircle, Edit, Smartphone } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { EBaseStatus } from "@/enum/base";
import { DeviceDialogProps } from "@/types/dialog";
import { ErrorResponse } from "@/types/error";
import { cn } from "@/lib/utils";
import { FormBaseStatusSelectField, FormFooterActions } from "@/components/form";
import { parseErrors } from "@/utils";
import { createDeviceType, updateDeviceType } from "@/services/device.service";
import { deviceTypeSchema } from "@/schema/device.schema";

const initialFormData = {
    name: "",
    description: "",
    status: EBaseStatus.Active,
    isMobileDevice: false,
};

const DeviceTypeDialog = ({ open, onOpenChange, onSuccess, deviceType }: DeviceDialogProps) => {
    const { toast } = useToast();
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState(initialFormData);
    const [focusedField, setFocusedField] = useState<string | null>(null);
    const [validFields, setValidFields] = useState<Record<string, boolean>>({});
    const nameInputRef = useRef<HTMLInputElement>(null);
    const [submitted, setSubmitted] = useState(false);
    const [errors, setErrors] = useState<Record<string, string>>({});

    const isUpdate = !!deviceType;

    // Reset form khi dialog đóng
    useEffect(() => {
        if (!open) {
            setFormData(initialFormData);
            setValidFields({});
            setErrors({});
            setSubmitted(false);
            setFocusedField(null);
        }
    }, [open]);

    // Điền dữ liệu khi chỉnh sửa
    useEffect(() => {
        if (deviceType) {
            setFormData({
                name: deviceType.name,
                description: deviceType.description || "",
                status: deviceType.status || EBaseStatus.Active,
                isMobileDevice: deviceType.isMobileDevice ?? false,
            });
            setValidFields({
                name: deviceType.name.trim().length >= 2 && deviceType.name.length <= 100,
                description: (deviceType.description || "").length <= 450,
            });
        }
    }, [deviceType]);

    // Phím tắt Ctrl+Enter để submit
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (!open) return;
            if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
                e.preventDefault();
                handleSubmit(e as any);
            }
        };
        document.addEventListener("keydown", handleKeyDown);
        return () => document.removeEventListener("keydown", handleKeyDown);
    }, [open, formData]);

    const validateField = (field: string, value: string) => {
        const newValidFields = { ...validFields };
        switch (field) {
            case "name":
                newValidFields.name = value.trim().length >= 2 && value.length <= 100;
                break;
            case "description":
                newValidFields.description = value.length <= 450;
                break;
        }
        setValidFields(newValidFields);
    };

    const handleChange = (field: string, value: string | boolean) => {
        if (field === "description" && typeof value === "string" && value.length > 450) {
            value = value.substring(0, 450);
        }
        setFormData((prev) => ({ ...prev, [field]: value }));
        if (field === "name" || field === "description") {
            validateField(field, value as string);
        }
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: "" }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true)

        const validationResult = deviceTypeSchema.safeParse(formData);
        if (!validationResult.success) {
            const parsedErrors = parseErrors(validationResult.error)
            setErrors(parsedErrors)
            return
        }
        setLoading(true);
        setErrors({});

        try {
            const data = {
                name: formData.name,
                description: formData.description || undefined,
                status: formData.status,
                isMobileDevice: formData.isMobileDevice,
            };
            if (deviceType) {
                await updateDeviceType(deviceType.deviceTypeId, data);
            } else {
                await createDeviceType(data);
            }

            toast({
                title: "🎉 Thành công",
                description: isUpdate ? "Cập nhật loại thiết bị thành công" : "Thêm loại thiết bị mới thành công",
            });
            onSuccess?.();
        } catch (error) {
            const err = error as ErrorResponse;
            console.error("Lỗi khi xử lý loại thiết bị:", error);
            toast({
                title: "Lỗi khi xử lý loại thiết bị",
                description: err.message,
                variant: "destructive",
            });
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[600px] p-0 border-0 bg-white backdrop-blur-xl shadow-2xl max-h-[90vh] overflow-y-auto hide-scrollbar">
                <DialogTitle className="sr-only">
                    {isUpdate ? "Cập nhật Loại Thiết Bị" : "Tạo Loại Thiết Bị Mới"}
                </DialogTitle>
                <DialogDescription className="sr-only">
                    Form thêm mới hoặc cập nhật loại thiết bị. Nhập tên, mô tả và trạng thái.
                </DialogDescription>
                {/* Header */}
                <div className="relative overflow-hidden bg-primary-100 rounded-tl-2xl rounded-tr-2xl">
                    <div className="absolute inset-0"></div>
                    <div className="relative px-8 py-6 border-b border-primary-300">
                        <div className="flex items-center space-x-4">
                            <div className="w-14 h-14 bg-gradient-to-r from-primary-400 to-primary-500 rounded-2xl flex items-center justify-center shadow-lg">
                                {isUpdate ? <Edit className="w-7 h-7 text-primary-100" /> : <MapPin className="w-7 h-7 text-primary-100" />}
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">
                                    {isUpdate ? "Cập nhật Loại Thiết Bị" : "Tạo Loại Thiết Bị Mới"}
                                </h1>
                                <p className="text-gray-500">
                                    {isUpdate ? "Chỉnh sửa thông tin loại thiết bị" : "Thêm loại thiết bị mới vào hệ thống"}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                <form onSubmit={handleSubmit} className="px-8 py-8 pt-2 space-y-8">
                    <div className="space-y-1">
                        <div className="flex items-center space-x-2 mb-2">
                            <MapPin className="w-4 h-4 text-primary-300" />
                            <Label className="text-sm font-medium text-gray-700 asterisk">
                                Tên Loại Thiết Bị
                            </Label>
                        </div>

                        <div className="relative group">
                            <Input
                                ref={nameInputRef}
                                placeholder="Ví dụ: Device check-in, Device thanh toán..."
                                value={formData.name}
                                onChange={(e) => handleChange("name", e.target.value)}
                                onFocus={() => setFocusedField("name")}
                                onBlur={() => setFocusedField(null)}
                                disabled={loading}
                                className={cn(
                                    "h-12 text-base px-4 border-2 transition-all duration-300 bg-white/80 backdrop-blur-sm pr-10",
                                    focusedField === "name" && "border-primary-300 ring-4 ring-primary-100 shadow-lg scale-[1.02]",
                                )}
                            />
                            {submitted && errors.name && (
                                <p className="text-red-500 text-xs mt-1">{errors.name}</p>
                            )}
                        </div>

                        <div className="flex justify-between items-center text-xs">
                            <span
                                className={cn(
                                    "transition-colors",
                                    !validFields.name && formData.name ? "text-red-500" : "text-gray-500",
                                )}
                            >
                                {!validFields.name && formData.name
                                    ? formData.name.trim().length < 2
                                        ? "Tên phải có ít nhất 2 ký tự"
                                        : formData.name.length > 100
                                            ? "Tên không được vượt quá 100 ký tự"
                                            : "Tên không hợp lệ"
                                    : ""}
                            </span>
                            <span
                                className={cn("transition-colors", formData.name.length > 80 ? "text-orange-500" : "text-gray-400")}
                            >
                                {formData.name.length}/100
                            </span>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <FormBaseStatusSelectField
                            label="Trạng thái"
                            value={formData.status}
                            onChange={(value) => handleChange("status", value as EBaseStatus)}
                            placeholder="Chọn trạng thái"
                            error={errors.status}
                            focusedField={focusedField}
                            setFocusedField={setFocusedField}
                            submitted={submitted}
                        />

                        <div className="space-y-3">
                            <div className="flex items-center space-x-2 mb-2">
                                <Smartphone className="w-4 h-4 text-primary-300" />
                                <Label className="text-sm font-medium text-gray-700 asterisk">
                                    Loại Thiết Bị
                                </Label>
                            </div>
                            <div className="flex items-center space-x-3 p-4 border-2 rounded-lg bg-white/80 backdrop-blur-sm border-gray-200">
                                <Switch
                                    checked={formData.isMobileDevice}
                                    onCheckedChange={(checked) => handleChange("isMobileDevice", checked)}
                                    disabled={loading}
                                />
                                <div className="flex-1">
                                    <Label className="text-sm font-medium cursor-pointer">
                                        {formData.isMobileDevice ? "Thiết bị di động" : "Thiết bị cố định"}
                                    </Label>
                                    <p className="text-xs text-gray-500 mt-1">
                                        {formData.isMobileDevice 
                                            ? "Thiết bị có thể di chuyển được" 
                                            : "Thiết bị được lắp đặt cố định"}
                                    </p>
                                </div>
                            </div>
                            {submitted && errors.isMobileDevice && (
                                <p className="text-red-500 text-xs mt-1">{errors.isMobileDevice}</p>
                            )}
                        </div>
                    </div>

                    <div className="space-y-3">
                        <div className="flex items-center space-x-2 mb-2">
                            <Edit className="w-4 h-4 text-primary-300" />
                            <Label className="text-sm font-medium text-gray-700">Mô tả</Label>
                        </div>

                        <div className="relative group">
                            <Textarea
                                placeholder="Mô tả chi tiết về loại thiết bị..."
                                value={formData.description}
                                onChange={(e) => handleChange("description", e.target.value)}
                                onFocus={() => setFocusedField("description")}
                                onBlur={() => setFocusedField(null)}
                                disabled={loading}
                                className={cn(
                                    "min-h-[100px] text-base p-4 border-2 transition-all duration-300 bg-white/80 backdrop-blur-sm resize-none",
                                    focusedField === "description" && "border-primary-300 ring-4 ring-primary-100 shadow-lg scale-[1.01]",
                                )}
                            />
                        </div>

                        <div className="flex justify-between items-center text-xs">
                            <span className="text-gray-500">Mô tả giúp phân biệt loại thiết bị này với các loại khác</span>
                            <span
                                className={cn(
                                    "transition-colors",
                                    formData.description.length > 400 ? "text-orange-500" : "text-gray-400",
                                )}
                            >
                                {formData.description.length}/450
                            </span>
                        </div>
                    </div>

                    <FormFooterActions
                        onCancel={() => onOpenChange(false)}
                        onSubmit={handleSubmit}
                        loading={loading}
                        isUpdate={isUpdate}
                    />
                </form>
            </DialogContent>
        </Dialog>
    );
};

export default DeviceTypeDialog;