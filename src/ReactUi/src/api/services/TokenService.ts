/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TokenService {

    /**
     * @param formData 
     * @returns any Success
     * @throws ApiError
     */
    public static token(
formData?: {
code?: string;
code_verifier?: string;
},
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Token',
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }

}
