import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { TdDialogService } from "@covalent/core";
import { first } from "rxjs/operators";

import { AuthService } from "./services/auth.service";

@Component({
    template: "Please wait, logging in..",
})
export class LoginComponent implements OnInit {
    constructor(private authService: AuthService, private router: Router, private activatedRoute: ActivatedRoute, private dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            const params = await this.activatedRoute.params.pipe(first()).toPromise();
            await this.authService.getNewToken(params.scheme);
            this.router.navigate(["/"]);
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to login",
                message: e.toString(),
            });
        }
    }
}
