import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { first } from "rxjs/operators";

import { ITeamSpeakChannel, ITeamSpeakClient } from "../app.data";

@Injectable()
export class OnlineService {
    constructor(private httpClient: HttpClient) { }
    public getClients() {
        return this.httpClient.get<ITeamSpeakClient[]>("/api/online/client").pipe(first()).toPromise();
    }
    public getChannels() {
        return this.httpClient.get<ITeamSpeakChannel[]>("/api/online/channel").pipe(first()).toPromise();
    }
}
