/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Instance } from '../models/Instance';
import type { ServerInformation } from '../models/ServerInformation';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class InstanceService {
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceStartServer(
        id?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/StartServer',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceStopServer(
        id?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/StopServer',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiInstanceRemoveServer(
        id?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Instance/RemoveServer',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns ServerInformation OK
     * @throws ApiError
     */
    public static getApiInstanceGetServerInformation(
        id?: string,
    ): CancelablePromise<ServerInformation> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/GetServerInformation',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns Instance OK
     * @throws ApiError
     */
    public static getApiInstanceGetInstance(
        id?: string,
    ): CancelablePromise<Instance> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/GetInstance',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns Instance OK
     * @throws ApiError
     */
    public static getApiInstanceGetInstances(): CancelablePromise<Array<Instance>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/GetInstances',
        });
    }
    /**
     * @returns Instance OK
     * @throws ApiError
     */
    public static getApiInstanceCreateEmptyInstance(): CancelablePromise<Instance> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/CreateEmptyInstance',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiInstanceCreateServer(
        requestBody?: Instance,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Instance/CreateServer',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static putApiInstanceUpdateInstance(
        requestBody?: Instance,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Instance/UpdateInstance',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param playerGuid
     * @param instanceId
     * @param reason
     * @param duration
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceBanPlayer(
        playerGuid?: string,
        instanceId?: string,
        reason?: string,
        duration?: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/BanPlayer',
            query: {
                'playerGuid': playerGuid,
                'instanceId': instanceId,
                'reason': reason,
                'duration': duration,
            },
        });
    }
    /**
     * @param playerGuid
     * @param instanceId
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceUnbanPlayer(
        playerGuid?: string,
        instanceId?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/UnbanPlayer',
            query: {
                'playerGuid': playerGuid,
                'instanceId': instanceId,
            },
        });
    }
    /**
     * @param playerGuid
     * @param instanceId
     * @param reason
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceKickPlayer(
        playerGuid?: string,
        instanceId?: string,
        reason?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/KickPlayer',
            query: {
                'playerGuid': playerGuid,
                'instanceId': instanceId,
                'reason': reason,
            },
        });
    }
    /**
     * @param playerGuid
     * @param instanceId
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceWhitelistPlayer(
        playerGuid?: string,
        instanceId?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/WhitelistPlayer',
            query: {
                'playerGuid': playerGuid,
                'instanceId': instanceId,
            },
        });
    }
    /**
     * @param playerGuid
     * @param instanceId
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInstanceUnwhitelistPlayer(
        playerGuid?: string,
        instanceId?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Instance/UnwhitelistPlayer',
            query: {
                'playerGuid': playerGuid,
                'instanceId': instanceId,
            },
        });
    }
}
