import { Component, OnInit } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";
import { TdDialogService } from "@covalent/core/dialogs";

import { IAuthScheme, IClientUser } from "../app.data";
import { AuthService } from "../services/auth.service";
import { UserService } from "../services/user.service";

@Component({
    templateUrl: "./account-dialog.template.html",
    styleUrls: ["./account-dialog.style.scss"],
})
export class AccountDialogComponent implements OnInit {
    public schemes?: IAuthScheme[];
    public selected?: IAuthScheme;
    public loading = false;
    public get user() {
        return this.authService.getUser();
    }
    public get userSchemes() {
        return this.user ? this.user.Authentication : {};
    }
    public get onlyScheme() {
        return Object.keys(this.userSchemes).length <= 1;
    }
    constructor(public dialogRef: MatDialogRef<AccountDialogComponent>, public authService: AuthService, public userService: UserService, private dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            this.loading = true;
            this.schemes = await this.authService.getAuthSchemes();
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to load auth schemes",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public openProfile(scheme: IAuthScheme) {
        if (this.userSchemes[scheme.Id] && scheme.ProfileUrl) {
            window.open(`${scheme.ProfileUrl}${this.userSchemes[scheme.Id]}`, "_blank");
        }
    }

    public login(schemeId: string) {
        window.location.replace(`/api/auth/login/${schemeId}`);
    }

    public async logout() {
        try {
            this.loading = true;
            const allDevices = await this.dialogService.openConfirm({
                acceptButton: "All devices",
                cancelButton: "Just this one",
                message: "Do you want to log out of all devices or just this one?",
                title: "Logout",
            }).afterClosed().toPromise();
            this.dialogRef.close();
            await this.authService.logout(allDevices);
            this.dialogService.openAlert({
                title: "Logout",
                message: allDevices ? "Logout successful, note that other devices may take up to 15 minutes to log out." : "Logout successful.",
            });
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to logout",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public async deleteAuthScheme(scheme: string) {
        try {
            this.loading = true;
            await this.userService.deleteAuthScheme(scheme);
        } catch (e) {
            this.dialogService.openAlert({
                title: `Failed to delete auth scheme ${scheme}`,
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }
}
