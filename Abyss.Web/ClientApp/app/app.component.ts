import { Component } from "@angular/core";

import { MatDialog } from "@angular/material";
import { AuthService } from "./services/auth.service";
import { AccountDialogComponent } from "./shared/account-dialog.component";

@Component({
    selector: "abyss",
    templateUrl: "./app.template.html",
    styleUrls: ["./app.style.scss"],
})
export class AppComponent {
    constructor(public authService: AuthService, private dialog: MatDialog) { }

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
