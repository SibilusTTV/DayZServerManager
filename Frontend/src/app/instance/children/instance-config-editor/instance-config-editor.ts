import {Component, signal, WritableSignal} from '@angular/core';
import {MatButton, MatIconButton} from '@angular/material/button';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatIcon} from '@angular/material/icon';
import {CustomMessage, Instance, InstanceService, Mod} from '../../../../api';
import {ActivatedRoute} from '@angular/router';
import InstanceConfig from '../../../../components/instance-config';

@Component({
  selector: 'instance-config-editor',
  templateUrl: './instance-config-editor.html',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    MatIconButton,
    MatIcon,
    InstanceConfig
  ],
})
export class InstanceConfigEditor {

  public clientMods: WritableSignal<Mod[]> = signal([]);
  public serverMods: WritableSignal<Mod[]> = signal([]);
  public customMessages: WritableSignal<CustomMessage[]> = signal([]);

  public id: WritableSignal<string> = signal("");
  public instanceId: WritableSignal<number> = signal(0);
  public serverFolder: WritableSignal<string> = signal("");
  public hostName: WritableSignal<string> = signal("");
  public missionName: WritableSignal<string> = signal("");
  public vanillaMissionName: WritableSignal<string> = signal("");
  public missionTemplateName: WritableSignal<string> = signal("");
  public serverConfigName: WritableSignal<string> = signal("");
  public profileName: WritableSignal<string> = signal("");
  public schedulerConfigName: WritableSignal<string> = signal("");
  public steamPort: WritableSignal<number> = signal(0);
  public serverPort: WritableSignal<number> = signal(0);
  public steamQueryPort: WritableSignal<number> = signal(0);
  public rConPort: WritableSignal<number> = signal(0);
  public rConPassword: WritableSignal<string> = signal("");
  public cpuCount: WritableSignal<number> = signal(0);
  public noFilePatching: WritableSignal<boolean> = signal(false);
  public doLogs: WritableSignal<boolean> = signal(false);
  public adminLog: WritableSignal<boolean> = signal(false);
  public freezeCheck: WritableSignal<boolean> = signal(false);
  public netLog: WritableSignal<boolean> = signal(false);
  public limitFPS: WritableSignal<number> = signal(0);
  public mapName: WritableSignal<string> = signal("");
  public restartOnUpdate: WritableSignal<boolean> = signal(false);
  public restartInterval: WritableSignal<number> = signal(0);
  public autoStartServer: WritableSignal<boolean> = signal(false);
  public makeBackups: WritableSignal<boolean> = signal(false);
  public deleteBackups: WritableSignal<boolean> = signal(false);
  public backupPath: WritableSignal<string> = signal("");
  public maxKeepTime: WritableSignal<number> = signal(0);

  constructor(private route: ActivatedRoute){
    this.route.params.subscribe(params => {
      this.id.set(params["id"]);
      this.LoadInstanceConfig();
    });
  }

  public LoadInstanceConfig() {
    InstanceService.getApiInstanceGetInstance(this.id()).then(instanceConfig => {

      this.clientMods.set(instanceConfig.clientMods ?? []);
      this.serverMods.set(instanceConfig.serverMods ?? []);
      this.customMessages.set(instanceConfig.customMessages ?? []);

      this.instanceId.set(instanceConfig.instanceId ?? 0);
      this.serverFolder.set(instanceConfig.serverFolder ?? "");
      this.hostName.set(instanceConfig.hostName ?? "");
      this.missionName.set(instanceConfig.missionName ?? "");
      this.vanillaMissionName.set(instanceConfig.vanillaMissionName ?? "");
      this.missionTemplateName.set(instanceConfig.missionTemplateName ?? "");
      this.serverConfigName.set(instanceConfig.serverConfigName ?? "");
      this.profileName.set(instanceConfig.profileName ?? "");
      this.steamPort.set(instanceConfig.steamPort ?? 0);
      this.serverPort.set(instanceConfig.serverPort ?? 0);
      this.steamQueryPort.set(instanceConfig.steamQueryPort ?? 0);
      this.rConPort.set(instanceConfig.rConPort ?? 0);
      this.rConPassword.set(instanceConfig.rConPassword ?? "");
      this.cpuCount.set(instanceConfig.cpuCount ?? 0);
      this.noFilePatching.set(instanceConfig.noFilePatching ?? false);
      this.doLogs.set(instanceConfig.doLogs ?? false);
      this.adminLog.set(instanceConfig.adminLog ?? false);
      this.freezeCheck.set(instanceConfig.freezeCheck ?? false);
      this.netLog.set(instanceConfig.netLog ?? false);
      this.limitFPS.set(instanceConfig.limitFPS ?? 0);
      this.mapName.set(instanceConfig.mapName ?? "");
      this.restartOnUpdate.set(instanceConfig.restartOnUpdate ?? false);
      this.restartInterval.set(instanceConfig.restartInterval ?? 0);
      this.autoStartServer.set(instanceConfig.autoStartServer ?? false);
      this.makeBackups.set(instanceConfig.makeBackups ?? false);
      this.deleteBackups.set(instanceConfig.deleteBackups ?? false);
      this.backupPath.set(instanceConfig.backupPath ?? "");
      this.maxKeepTime.set(instanceConfig.maxKeepTime ?? 0);
    });
  }

  onSaveClick(): void {
    const instanceConfig: Instance = {
      id: this.id(),
      instanceId: this.instanceId(),
      serverFolder: this.serverFolder(),
      hostName: this.hostName(),
      missionName: this.missionName(),
      vanillaMissionName: this.vanillaMissionName(),
      missionTemplateName: this.missionTemplateName(),
      serverConfigName: this.serverConfigName(),
      profileName: this.profileName(),
      steamPort: this.steamPort(),
      serverPort: this.serverPort(),
      steamQueryPort: this.steamQueryPort(),
      rConPort: this.rConPort(),
      rConPassword: this.rConPassword(),
      cpuCount: this.cpuCount(),
      noFilePatching: this.noFilePatching(),
      doLogs: this.doLogs(),
      adminLog: this.adminLog(),
      freezeCheck: this.freezeCheck(),
      netLog: this.netLog(),
      limitFPS: this.limitFPS(),
      mapName: this.mapName(),
      restartOnUpdate: this.restartOnUpdate(),
      restartInterval: this.restartInterval(),
      autoStartServer: this.autoStartServer(),
      makeBackups: this.makeBackups(),
      deleteBackups: this.deleteBackups(),
      backupPath: this.backupPath(),
      maxKeepTime: this.maxKeepTime(),
      clientMods: this.clientMods(),
      serverMods: this.serverMods(),
      customMessages: this.customMessages()
    }

    InstanceService.putApiInstanceUpdateInstance(instanceConfig).then();
  }
}
