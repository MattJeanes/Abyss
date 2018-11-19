import { Component, OnInit } from "@angular/core";
import { TdDialogService } from "@covalent/core";

import { MatDialog } from "@angular/material";
import { AuthService } from "./services/auth.service";
import { AccountDialogComponent } from "./shared/account-dialog.component";

@Component({
    selector: "abyss",
    templateUrl: "./app.template.html",
    styleUrls: ["./app.style.scss"],
})
export class AppComponent implements OnInit {
    constructor(public authService: AuthService, private dialog: MatDialog, private dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            if (this.authService.isLoggedIn()) {
                await this.authService.getNewToken();
            }
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to get new auth token, forcing logout",
                message: e.toString(),
            });
            try {
                await this.authService.logout();
            } catch (e) {
                this.dialogService.openAlert({
                    title: "Failed to log out",
                    message: e.toString(),
                });
            }
        }
    }

    public get username() {
        const user = this.authService.getUser();
        if (user) {
            return user.Name;
        } else {
            return undefined;
        }
    }

    public showAccountDialog() {
        this.dialog.open(AccountDialogComponent);
    }
}
