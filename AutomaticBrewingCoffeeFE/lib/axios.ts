import { HttpStatus } from "@/enum/http";
import axios from "axios";
import { toastService } from "@/utils";
import Cookies from "js-cookie"
import { logout, refreshToken } from "@/services/auth.service";
import { Path } from "@/constants/path.constant";

export const axiosInstance = axios.create({
    // In development, point to the local Next.js proxy so we avoid CORS.
    baseURL: process.env.NODE_ENV === 'development' ? '/api/v1' : process.env.NEXT_PUBLIC_API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    },
    timeout: 300000,
    timeoutErrorMessage: `Connection is timeout exceeded`
})

let isTokenExpired = false;
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (token) {
            prom.resolve(token);
        } else {
            prom.reject(error);
        }
    });
    failedQueue = [];
};

axiosInstance.interceptors.request.use(
    (config) => {
        const token = Cookies.get('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

axiosInstance.interceptors.response.use(
    (response) => {
        console.log(response);
        if (response.status === HttpStatus.Success || response.status === HttpStatus.Created || response.status === HttpStatus.Accepted) {
            return response.data;
        }
    },
    async (error) => {
        // Log the entire error for debugging; error.response may be undefined for network errors
        console.log(error);
        const originalRequest = error.config;
        const response = error && error.response ? error.response : null;
        const data = response ? response.data : null;
        if (response) {
            // Xử lý lỗi 401
            if (error.response.status === HttpStatus.Unauthorized && !originalRequest._retry && error.response?.message?.includes("làm mới")) {
                if (isRefreshing) {
                    return new Promise((resolve, reject) => {
                        failedQueue.push({ resolve, reject });
                    })
                        .then((token) => {
                            originalRequest.headers['Authorization'] = `Bearer ${token}`;
                            return axiosInstance(originalRequest);
                        })
                        .catch((err) => {
                            return Promise.reject(err);
                        });
                }

                originalRequest._retry = true;
                isRefreshing = true;

                try {
                    const newAccessToken = await refreshToken();
                    processQueue(null, newAccessToken);
                    originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;
                    return axiosInstance(originalRequest);
                } catch (refreshError) {
                    processQueue(refreshError, null);
                    logout();
                    toastService.show({
                        title: "Hệ thống gặp trục trặc",
                        description: "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.",
                        variant: "warning"
                    });
                    window.location.href = Path.LOGIN;
                    return Promise.reject(refreshError);
                } finally {
                    isRefreshing = false;
                }
            }
            else {
                switch (error.response.status) {
                    case HttpStatus.Forbidden: {
                        if (!isTokenExpired) {
                            isTokenExpired = true
                            toastService.show({
                                variant: "destructive",
                                title: "Hệ thống gặp trục trặc",
                                description: `${data?.message || data?.Message}`,
                            })
                            //   toast.error(data.message);
                            //   setTimeout(() => {
                            //     if (user) {
                            //       window.location.href = PATHS.HOME
                            //     } else {
                            //       return;
                            //     }
                            //     localStorage.clear();
                            //     isTokenExpired = false;
                            //   }, 1300);
                        }
                        break;
                    }

                    case HttpStatus.NotFound:
                        toastService.show({
                            variant: "destructive",
                            title: "Hệ thống gặp trục trặc",
                            description: `${data?.message || data?.Message}`,
                        })
                        // toast.error(data.message || data.Message);                                           
                        //     switch(user.role){
                        //       case "member":
                        //         window.location.href = Path.NOTFOUND;
                        //         break;
                        //       case "admin":
                        //         window.location.href = '/admin/404';
                        //         break;

                        //       case "staff":
                        //         window.location.href = "/staff/404";
                        //         break;
                        //       default:
                        //         window.location.href = Path.HOME;
                        //         break;
                        //     }
                        break;

                    case HttpStatus.InternalServerError:
                        toastService.show({
                            variant: "destructive",
                            title: "Hệ thống gặp trục trặc",
                            description: `${data?.message || data?.Message || data}`,
                        })
                        // window.location.href = WebPath.INTERNAL_SERVER_ERROR;
                        break;

                    default:
                        console.log(data?.message)
                        toastService.show({
                            variant: "destructive",
                            title: "Hệ thống gặp trục trặc",
                            description: `${data?.message || data?.Message || "Server bị lỗi"}`,
                        })
                        break;
                }
            }

            return Promise.reject(data || response);
        } else {
            //   toast.error('Network error');
            // Network or unknown error (no response) - return the original error for upstream handling
            toastService.show({
                variant: "destructive",
                title: "Lỗi kết nối",
                description: "Không thể kết nối tới server. Vui lòng kiểm tra kết nối mạng hoặc thử lại sau.",
            });
            return Promise.reject(error);
        }
    }
);