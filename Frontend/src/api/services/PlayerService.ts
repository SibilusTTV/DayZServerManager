/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServerPlayerInformation } from '../models/ServerPlayerInformation';
import type { User } from '../models/User';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class PlayerService {
    /**
     * @returns User OK
     * @throws ApiError
     */
    public static getApiPlayerGetPlayers(): CancelablePromise<Array<User>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetPlayers',
        });
    }
    /**
     * @param id
     * @returns User OK
     * @throws ApiError
     */
    public static getApiPlayerGetPlayer(
        id?: string,
    ): CancelablePromise<User> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetPlayer',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns ServerPlayerInformation OK
     * @throws ApiError
     */
    public static getApiPlayerGetServerPlayerInformation(
        id?: string,
    ): CancelablePromise<Array<ServerPlayerInformation>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetServerPlayerInformation',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param playerId
     * @param instanceId
     * @param isWhitelisted
     * @param isBanned
     * @param roleName
     * @returns any OK
     * @throws ApiError
     */
    public static postApiPlayerCreateServerPlayer(
        playerId?: string,
        instanceId?: string,
        isWhitelisted?: boolean,
        isBanned?: boolean,
        roleName?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Player/CreateServerPlayer',
            query: {
                'playerId': playerId,
                'instanceId': instanceId,
                'isWhitelisted': isWhitelisted,
                'isBanned': isBanned,
                'roleName': roleName,
            },
        });
    }
    /**
     * @param instanceId
     * @returns string OK
     * @throws ApiError
     */
    public static getApiPlayerGetRoleNames(
        instanceId?: string,
    ): CancelablePromise<Array<string>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetRoleNames',
            query: {
                'instanceId': instanceId,
            },
        });
    }
    /**
     * @param serverPlayerId
     * @param playerGuid
     * @param instanceId
     * @param roleName
     * @returns any OK
     * @throws ApiError
     */
    public static postApiPlayerSetRole(
        serverPlayerId?: string,
        playerGuid?: string,
        instanceId?: string,
        roleName?: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Player/SetRole',
            query: {
                'serverPlayerId': serverPlayerId,
                'playerGuid': playerGuid,
                'instanceId': instanceId,
                'roleName': roleName,
            },
        });
    }
}
