import { Component, OnInit } from "@angular/core";
import { MatDialogRef } from "@angular/material";

import { IAuthScheme } from "../app.data";
import { AuthService } from "../services/auth.service";
import { UserService } from "../services/user.service";

@Component({
    templateUrl: "./account-dialog.template.html",
    styleUrls: ["./account-dialog.style.scss"],
})
export class AccountDialogComponent implements OnInit {
    public schemes: IAuthScheme[];
    public selected: IAuthScheme;
    public get user() {
        return this.authService.getUser();
    }
    public get userSchemes() {
        return this.user ? this.user.Authentication : [];
    }
    public get onlyScheme() {
        return Object.keys(this.userSchemes).length <= 1;
    }
    constructor(public dialogRef: MatDialogRef<AccountDialogComponent>, public authService: AuthService, public userService: UserService) { }

    public async ngOnInit() {
        this.schemes = await this.authService.getAuthSchemes();
    }

    public openProfile(scheme: IAuthScheme) {
        window.open(`${scheme.ProfileUrl}${this.userSchemes[scheme.Id]}`, "_blank");
    }

    public login(schemeId: string) {
        window.location.replace(`/api/auth/login/${schemeId}`);
    }
}
