import { Component } from "@angular/core";

import { Permissions } from "../app.data";
import { AuthService } from "../services/auth.service";
import { ErrorService } from "../services/error.service";

@Component({
    templateUrl: "./home.template.html",
    styleUrls: ["./home.style.scss"],
})
export class HomeComponent {
    public Permissions = Permissions;
    constructor(public authService: AuthService, public errorService: ErrorService) { }
}
