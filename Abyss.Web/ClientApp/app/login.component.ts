import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { first } from "rxjs/operators";

import { AppService } from "./app.service";

@Component({
    template: "Please wait, logging in..",
})
export class LoginComponent implements OnInit {
    constructor(private appService: AppService, private router: Router, private activatedRoute: ActivatedRoute) { }

    public async ngOnInit() {
        const params = await this.activatedRoute.params.pipe(first()).toPromise();
        console.log(params);
        const token = await this.appService.getNewToken(params.scheme);
        this.appService.setToken(token.Token);
        this.router.navigate(["/"]);
    }
}
