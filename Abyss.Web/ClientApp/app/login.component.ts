import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { first } from "rxjs/operators";

import { AuthService } from "./services/auth.service";

@Component({
    template: "Please wait, logging in..",
})
export class LoginComponent implements OnInit {
    constructor(private authService: AuthService, private router: Router, private activatedRoute: ActivatedRoute) { }

    public async ngOnInit() {
        const params = await this.activatedRoute.params.pipe(first()).toPromise();
        const token = await this.authService.getNewToken(params.scheme);
        this.authService.setToken(token.Token);
        this.router.navigate(["/"]);
    }
}
