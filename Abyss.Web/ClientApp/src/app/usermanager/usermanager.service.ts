import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { first } from "rxjs/operators";

import { IUser } from "../app.data";

@Injectable()
export class UserManagerService {
    constructor(private httpClient: HttpClient) { }
    public getUsers() {
        return this.httpClient.get<IUser[]>("/api/user").pipe(first()).toPromise();
    }
}
