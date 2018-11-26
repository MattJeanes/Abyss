import { Component, OnInit } from "@angular/core";
import { TdDialogService } from "@covalent/core";

import { IUser } from "../app.data";
import { UserService } from "../services/user.service";

@Component({
    templateUrl: "./usermanager.template.html",
})
export class UserManagerComponent implements OnInit {
    public users: IUser[];

    constructor(public userService: UserService, public dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            this.users = await this.userService.getUsers();
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to load user manager",
                message: e.toString(),
            });
        }
    }
}
