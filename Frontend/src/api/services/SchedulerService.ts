/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SchedulerConfig } from '../models/SchedulerConfig';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SchedulerService {
    /**
     * @param instanceId
     * @returns SchedulerConfig OK
     * @throws ApiError
     */
    public static getApiSchedulerGetSchedulerConfig(
        instanceId?: number,
    ): CancelablePromise<SchedulerConfig> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Scheduler/GetSchedulerConfig',
            query: {
                'instanceId': instanceId,
            },
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiSchedulerCreateEditSchedulerConfig(
        requestBody?: SchedulerConfig,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Scheduler/CreateEditSchedulerConfig',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
