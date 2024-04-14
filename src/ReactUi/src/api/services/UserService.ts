/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GuildDto } from '../models/GuildDto';
import type { GuildPermissions } from '../models/GuildPermissions';
import type { ModerationDto } from '../models/ModerationDto';
import type { RoleDto } from '../models/RoleDto';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UserService {

    /**
     * @returns GuildDto Success
     * @throws ApiError
     */
    public static getApiUserGuilds(): CancelablePromise<Array<GuildDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/User/guilds',
        });
    }

    /**
     * @param guildId 
     * @returns ModerationDto Success
     * @throws ApiError
     */
    public static getApiUserModerations(
guildId?: string,
): CancelablePromise<Array<ModerationDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/User/moderations',
            query: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId 
     * @returns RoleDto Success
     * @throws ApiError
     */
    public static getApiUserRoles(
guildId?: string,
): CancelablePromise<Array<RoleDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/User/roles',
            query: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId 
     * @returns GuildPermissions Success
     * @throws ApiError
     */
    public static getApiUserPermissions(
guildId?: string,
): CancelablePromise<GuildPermissions> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/User/permissions',
            query: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId 
     * @param requestBody 
     * @returns RoleDto Success
     * @throws ApiError
     */
    public static patchApiUserPublicRoles(
guildId?: string,
requestBody?: Array<RoleDto>,
): CancelablePromise<Array<RoleDto>> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/User/public-roles',
            query: {
                'guildId': guildId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
