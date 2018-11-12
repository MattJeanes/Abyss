import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { MatDialogRef } from "@angular/material";
import { first } from "rxjs/operators";

import { IAuthScheme } from "../app.data";

@Component({
    templateUrl: "./login-dialog.template.html",
    styleUrls: ["./login-dialog.style.scss"],
})
export class LoginDialogComponent implements OnInit {
    public schemes: IAuthScheme[];
    public selected: IAuthScheme;
    constructor(public dialogRef: MatDialogRef<LoginDialogComponent>, private httpClient: HttpClient) { }

    public async ngOnInit() {
        this.schemes = await this.getAuthSchemes();
    }

    public getAuthSchemes() {
        return this.httpClient.get<IAuthScheme[]>("/auth/schemes").pipe(first()).toPromise();
    }
}
