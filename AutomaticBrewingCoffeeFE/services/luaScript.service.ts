import { BaseService } from "./base.service";
import { Api } from "@/constants/api.constant";
import { axiosInstance } from "@/lib/axios";

export interface LuaScript {
    scriptId: string;
    name: string;
    fileName: string | null;
    relativePath: string | null;
    description: string | null;
    version: string | null;
}

export const getLuaScripts = async (search?: string): Promise<LuaScript[]> => {
    try {
        // Axios interceptor đã unwrap response.data, nên response đã là BaseResult format
        const response = await axiosInstance.get(Api.LUA_SCRIPTS, {
            params: search ? { search } : {},
        }) as any;
        
        // API trả về format: { isSuccess, response: LuaScript[], message, statusCode, ... }
        // Vì interceptor đã unwrap, nên response chính là response.data từ axios
        const scripts = response?.response || [];
        
        // Debug log để kiểm tra
        console.log("LuaScripts response:", response);
        console.log("LuaScripts list:", scripts);
        
        return scripts;
    } catch (error) {
        console.error("Error fetching Lua scripts:", error);
        return [];
    }
};

export const uploadLuaScript = async (file: File, overwrite: boolean = false): Promise<{ ok: boolean; file: string; synced?: number }> => {
    const formData = new FormData();
    formData.append("file", file);

    try {
        // Axios interceptor đã trả về response.data trực tiếp
        const result = await axiosInstance.post(`${Api.LUA_SCRIPTS}/upload?overwrite=${overwrite}`, formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        }) as any;

        // Kiểm tra kết quả
        if (result && result.ok === false) {
            throw new Error(result.message || "Upload failed");
        }

        // API trả về: { ok: true, file: "filename.lua", synced: 1 }
        if (result && result.file) {
            return {
                ok: result.ok !== false,
                file: result.file,
                synced: result.synced
            };
        }

        // Fallback: nếu không có field file, dùng tên file gốc
        return {
            ok: true,
            file: file.name,
            synced: 0
        };
    } catch (error: any) {
        // Xử lý lỗi từ axios interceptor (đã được unwrap)
        const errorMessage = error?.message || error?.error || "Không thể upload file";
        throw new Error(errorMessage);
    }
};

export const syncLuaScripts = async (basePath?: string): Promise<{ synced: number }> => {
    const response = await BaseService.post({
        url: `${Api.LUA_SCRIPTS}/sync`,
        payload: {},
        params: basePath ? { basePath } : {},
    });
    return response as any;
};

