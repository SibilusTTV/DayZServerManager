import {Component, inject, OnDestroy, OnInit, signal, WritableSignal} from '@angular/core';
import {InstanceService, ServerInformation} from '../../../../api';
import {ActivatedRoute, Router} from '@angular/router';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {FormsModule} from '@angular/forms';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';

@Component({
  selector: ' instance-overview',
  imports: [
    MatIcon,
    MatIconButton,
    FormsModule,
    MatFormField,
    MatInput,
    MatLabel
  ],
  templateUrl: './overview.html',
  styleUrl: './overview.css'
})
export default class Overview implements OnInit, OnDestroy {
  private router = inject(Router);
  public instanceId: string = "";
  public serverInformation: WritableSignal<ServerInformation> = signal({});

  private timer: number = 5;

  constructor(private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.instanceId = params['id'];
      InstanceService.getApiInstanceGetServerInformation(this.instanceId).then(serverInformation => {
        this.serverInformation.set(serverInformation);
      });
    });
  }

  ngOnInit() {
    this.timer = setInterval(() => {
      InstanceService.getApiInstanceGetServerInformation(this.instanceId).then(serverInformation => {
        this.serverInformation.set(serverInformation);
      });
    }, 5000);
  }

  ngOnDestroy() {
    clearInterval(this.timer);
  }

  public onStartClick(): void {
    InstanceService.getApiInstanceStartServer(this.instanceId).then();
  }

  public onStopClick(): void {
    InstanceService.getApiInstanceStopServer(this.instanceId).then();
  }

  public onRemoveClicked(){
    InstanceService.deleteApiInstanceRemoveServer(this.instanceId).then(() => {
      this.router.navigate(['/']);
    });
  }
}
