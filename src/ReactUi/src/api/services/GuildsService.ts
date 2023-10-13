/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
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

}
