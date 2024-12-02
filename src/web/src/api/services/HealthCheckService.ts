/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class HealthCheckService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @returns boolean Success
     * @throws ApiError
     */
    public checkHealthAsync(): CancelablePromise<boolean> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/api/health',
        });
    }
    /**
     * @returns boolean Success
     * @throws ApiError
     */
    public checkHealthWithCacheAsync(): CancelablePromise<boolean> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/api/health-with-cache',
        });
    }
    /**
     * @returns any Success
     * @throws ApiError
     */
    public triggerRedisPublishing(): CancelablePromise<any> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/api/health-check-pub-sub',
        });
    }
}
