import {Component, inject, model, signal, WritableSignal} from '@angular/core';
import {
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';
import {MatButton} from '@angular/material/button';
import {SteamService} from '../../api';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'steam-guard-dialog',
  templateUrl: './steam-guard-dialog.html',
  imports: [
    MatDialogTitle,
    MatDialogContent,
    MatFormField,
    MatLabel,
    MatInput,
    MatDialogActions,
    MatButton,
    FormsModule
  ],
})
export class SteamGuardDialog {
  readonly dialogRef = inject(MatDialogRef<SteamGuardDialog>);
  public steamGuard: WritableSignal<string> = signal("");

  onNoClick(): void {
    this.dialogRef.close();
  }

  onOkClick(): void {
    SteamService.postApiSteamWriteSteamGuard(this.steamGuard()).then();
    this.dialogRef.close();
  }
}
