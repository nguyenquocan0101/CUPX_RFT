import { EExpressionType, EOperation, EWorkflowType } from "@/enum/workflow";
import { Product } from "./product";
import { DeviceModel } from "./device";

export interface WorkflowStep {
    name: string;
    type: string;
    deviceModelId: string;
    maxRetries: number;
    callbackWorkflowId: string;
    parameters: string;
    sequence: number;
    deviceModel: DeviceModel
    conditions: WorkflowCondition[];
    stepCode?: string;
    callbackStepCode?: string | null;
    // New fields for Workflow_Controll compatibility
    system?: "arm" | "iot";
    timeout?: number;
    requestJson?: string; // JSON string representation of request object
}

export interface Workflow {
    workflowId: string;
    productId: string | null;
    product: Product;
    name: string;
    description: string;
    type: EWorkflowType;
    steps: WorkflowStep[];
    kioskVersionId: string;
}

export interface WorkflowCondition {
    name: string;
    description: string;
    expression: {
        left: {
            type: EExpressionType;
            value: any;
        },
        operator: EOperation;
        right: {
            type: EExpressionType;
            value: any;
        }
    }
}