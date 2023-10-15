/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LogoutService {

    /**
     * @param token 
     * @param tokenTypeHint 
     * @param postLogoutRedirectUri 
     * @returns any Success
     * @throws ApiError
     */
    public static getLogout(
token?: string,
tokenTypeHint?: string,
postLogoutRedirectUri?: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Logout',
            query: {
                'token': token,
                'token_type_hint': tokenTypeHint,
                'post_logout_redirect_uri': postLogoutRedirectUri,
            },
        });
    }

}
