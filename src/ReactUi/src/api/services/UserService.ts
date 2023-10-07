/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GuildDto } from '../models/GuildDto';
import type { ModerationDto } from '../models/ModerationDto';

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

}
