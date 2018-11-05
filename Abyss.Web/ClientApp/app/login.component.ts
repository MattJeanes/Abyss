import { Component, OnInit } from "@angular/core";

import { Router } from "@angular/router";
import { AppService } from "./app.service";

@Component({
    template: "Please wait, logging in..",
})
export class LoginComponent implements OnInit {
    constructor(private appService: AppService, private router: Router) { }

    public async ngOnInit() {
        const token = await this.appService.getNewToken();
        this.appService.setToken(token.Token);
        this.router.navigate(["/"]);
    }
}
