import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first } from 'rxjs/operators';

import { IGPTRequest, IGPTResponse, IGPTModel } from '../app.data';

@Injectable()
export class GPTService {
    constructor(private httpClient: HttpClient) { }

    public generate(message: IGPTRequest): Promise<IGPTResponse> {
        return this.httpClient.post<IGPTResponse>('/api/gpt', message).pipe(first()).toPromise();
    }

    public getModels(): Promise<IGPTModel[]> {
        return this.httpClient.get<IGPTModel[]>('/api/gpt/models').pipe(first()).toPromise();
    }
}
