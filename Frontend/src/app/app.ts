import {Component, inject, isDevMode, OnDestroy, OnInit, signal} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {MatToolbar} from '@angular/material/toolbar';
import {MatButton} from '@angular/material/button';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import {OpenAPI, SteamService} from '../api';
import {MatDialog} from '@angular/material/dialog';
import {SteamGuardDialog} from './steam-guard-dialog/steam-guard-dialog';

// Register all Community features
ModuleRegistry.registerModules([AllCommunityModule]);

if (isDevMode()){
  OpenAPI.BASE = "http://localhost:5041";
}
else{
  OpenAPI.BASE = ".";
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatToolbar, MatButton],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('Frontend');
  readonly dialog = inject(MatDialog);
  private timer: number = 0;
  private steamGuardSend: number = 6;
  private dialogOpened = false;

  ngOnInit() {
    this.timer = setInterval(() => {
      SteamService.getApiSteamGetSteamInformation().then(data => {
        if (data.steamCmdStatus == "Steam Guard" && this.steamGuardSend > 5) {
          this.steamGuardSend = 0;
          this.openDialog();
        }
        else if (this.steamGuardSend < 6 && !this.dialogOpened){
          this.steamGuardSend++;
        }
      })
    }, 5000);
  }

  ngOnDestroy() {
    clearInterval(this.timer);
  }

  openDialog(): void {
    let dialogRef = undefined;
    if (!this.dialogOpened) dialogRef = this.dialog.open(SteamGuardDialog);
    this.dialogOpened = true;

    if (dialogRef) dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.dialogOpened = false;
    });
  }
}
