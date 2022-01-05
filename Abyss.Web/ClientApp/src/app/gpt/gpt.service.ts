import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { IGPTRequest, IGPTResponse, IGPTModel } from '../app.data';

@Injectable()
export class GPTService {
    constructor(private httpClient: HttpClient) { }

    public generate(message: IGPTRequest): Promise<IGPTResponse> {
        return firstValueFrom(this.httpClient.post<IGPTResponse>('/api/gpt', message));
    }

    public getModels(): Promise<IGPTModel[]> {
        return firstValueFrom(this.httpClient.get<IGPTModel[]>('/api/gpt/models'));
    }
}
