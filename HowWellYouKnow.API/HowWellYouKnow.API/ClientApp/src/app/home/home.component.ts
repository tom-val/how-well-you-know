import { Component } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  private hubConnection: signalR.HubConnection;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                          .withUrl('https://localhost:5001/gameState')
                            .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public addTransferChartDataListener = () => {
    this.hubConnection.on('be7a1b84-ba84-4ecb-b1b2-2df7eaa1e64c', (data) => {
      console.log(data);
    });
  }

  ngOnInit() {
    this.startConnection();
    this.addTransferChartDataListener();
  }

}
