import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first } from 'rxjs/operators';

import { IGPTMessage } from '../app.data';

@Injectable()
export class GPTService {
    constructor(private httpClient: HttpClient) { }
    public generate(message: string): Promise<IGPTMessage> {
        return this.httpClient.post<IGPTMessage>('/api/gpt', message).pipe(first()).toPromise();
    }
}
