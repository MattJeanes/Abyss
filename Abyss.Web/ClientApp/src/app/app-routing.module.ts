import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { LoginComponent } from "./login.component";
import { PageNotFoundComponent } from "./errors/not-found.component";

const routes: Routes = [
  {
    path: "",
    loadChildren: () => import("./home/home.module").then((m) => m.HomeModule),
    data: { pageTitle: "Home" },
  },
  {
    path: "login/:scheme",
    component: LoginComponent,
  },
  {
    path: "usermanager",
    loadChildren: () => import("./usermanager/usermanager.module").then((m) => m.UserManagerModule),
    data: { pageTitle: "User Manager" },
  },
  {
    path: "servermanager",
    loadChildren: () => import("./servermanager/servermanager.module").then((m) => m.ServerManagerModule),
    data: { pageTitle: "Server Manager" },
  },
  {
    path: "online",
    loadChildren: () => import("./online/online.module").then((m) => m.OnlineModule),
    data: { pageTitle: "Online" },
  },
  {
    path: "whosaid",
    loadChildren: () => import("./whosaid/whosaid.module").then((m) => m.WhoSaidModule),
    data: { pageTitle: "Who Said" },
  },
  {
    path: "**",
    component: PageNotFoundComponent,
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule { }
