/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HttpStatusCode } from '../models/HttpStatusCode';
import type { ServerConfig } from '../models/ServerConfig';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ServerConfigService {
    /**
     * @param instanceId
     * @returns ServerConfig OK
     * @throws ApiError
     */
    public static getApiServerConfigGet(
        instanceId?: string,
    ): CancelablePromise<ServerConfig> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/ServerConfig/Get',
            query: {
                'instanceId': instanceId,
            },
        });
    }
    /**
     * @param instanceId
     * @param requestBody
     * @returns HttpStatusCode OK
     * @throws ApiError
     */
    public static postApiServerConfigPost(
        instanceId?: string,
        requestBody?: ServerConfig,
    ): CancelablePromise<HttpStatusCode> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/ServerConfig/Post',
            query: {
                'instanceId': instanceId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
