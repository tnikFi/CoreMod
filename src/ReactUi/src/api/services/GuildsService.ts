/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ChannelDto } from '../models/ChannelDto';
import type { ModerationDto } from '../models/ModerationDto';
import type { ModerationDtoPaginatedResult } from '../models/ModerationDtoPaginatedResult';
import type { RoleDto } from '../models/RoleDto';
import type { UserDto } from '../models/UserDto';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class GuildsService {

    /**
     * @param guildId 
     * @returns number Success
     * @throws ApiError
     */
    public static getApiGuildsMemberCount(
guildId: string,
): CancelablePromise<number> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Guilds/{guildId}/member-count',
            path: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId 
     * @returns ChannelDto Success
     * @throws ApiError
     */
    public static getApiGuildsChannels(
guildId: string,
): CancelablePromise<Array<ChannelDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Guilds/{guildId}/channels',
            path: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId 
     * @param userId 
     * @returns UserDto Success
     * @throws ApiError
     */
    public static getApiGuildsUser(
guildId: string,
userId: string,
): CancelablePromise<UserDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Guilds/{guildId}/user/{userId}',
            path: {
                'guildId': guildId,
                'userId': userId,
            },
        });
    }

    /**
     * @param guildId 
     * @param page 
     * @param pageSize 
     * @returns ModerationDtoPaginatedResult Success
     * @throws ApiError
     */
    public static getApiGuildsModerations(
guildId: string,
page?: number,
pageSize: number = 25,
): CancelablePromise<ModerationDtoPaginatedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Guilds/{guildId}/moderations',
            path: {
                'guildId': guildId,
            },
            query: {
                'page': page,
                'pageSize': pageSize,
            },
        });
    }

    /**
     * @param guildId 
     * @param moderationId 
     * @param requestBody 
     * @returns ModerationDto Success
     * @throws ApiError
     */
    public static patchApiGuildsModerations(
guildId: string,
moderationId: number,
requestBody?: ModerationDto,
): CancelablePromise<ModerationDto> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/Guilds/{guildId}/moderations/{moderationId}',
            path: {
                'guildId': guildId,
                'moderationId': moderationId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param guildId 
     * @param moderationId 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteApiGuildsModerations(
guildId: string,
moderationId: number,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Guilds/{guildId}/moderations/{moderationId}',
            path: {
                'guildId': guildId,
                'moderationId': moderationId,
            },
        });
    }

    /**
     * @param guildId 
     * @returns RoleDto Success
     * @throws ApiError
     */
    public static getApiGuildsPublicRoles(
guildId: string,
): CancelablePromise<Array<RoleDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Guilds/{guildId}/public-roles',
            path: {
                'guildId': guildId,
            },
        });
    }

}
