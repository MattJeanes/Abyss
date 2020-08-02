import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first } from 'rxjs/operators';

import { IServer } from '../app.data';

@Injectable()
export class ServerManagerService {
    constructor(private httpClient: HttpClient) { }
    public getServers(): Promise<IServer[]> {
        return this.httpClient.get<IServer[]>('/api/server').pipe(first()).toPromise();
    }
    public start(serverId: string): Promise<boolean> {
        return this.httpClient.post<boolean>(`/api/server/start/${serverId}`, undefined).pipe(first()).toPromise();
    }
    public stop(serverId: string): Promise<boolean> {
        return this.httpClient.post<boolean>(`/api/server/stop/${serverId}`, undefined).pipe(first()).toPromise();
    }
}
