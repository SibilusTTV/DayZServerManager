/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SteamCredentials } from '../models/SteamCredentials';
import type { SteamInformation } from '../models/SteamInformation';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SteamService {
    /**
     * @returns SteamInformation OK
     * @throws ApiError
     */
    public static getApiSteamGetSteamInformation(): CancelablePromise<SteamInformation> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Steam/GetSteamInformation',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiSteamWriteSteamGuard(
        requestBody?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Steam/WriteSteamGuard',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns SteamCredentials OK
     * @throws ApiError
     */
    public static getApiSteamGetSteamCredentials(): CancelablePromise<SteamCredentials> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Steam/GetSteamCredentials',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiSteamSaveSteamCredentials(
        requestBody?: SteamCredentials,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Steam/SaveSteamCredentials',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
