/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SchedulerService {
    /**
     * @param serverPlayerId
     * @param reason
     * @param duration
     * @returns any OK
     * @throws ApiError
     */
    public static getApiSchedulerBanPlayer(
        serverPlayerId?: string,
        reason?: string,
        duration?: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Scheduler/BanPlayer',
            query: {
                'serverPlayerId': serverPlayerId,
                'reason': reason,
                'duration': duration,
            },
        });
    }
    /**
     * @param serverPlayerId
     * @param reason
     * @returns any OK
     * @throws ApiError
     */
    public static getApiSchedulerWhitelistPlayer(
        serverPlayerId?: string,
        reason?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Scheduler/WhitelistPlayer',
            query: {
                'serverPlayerId': serverPlayerId,
                'reason': reason,
            },
        });
    }
}
