import http from 'k6/http';
import { sleep } from 'k6';

export default function () {
    http.post('http://fantasticbike1.westeurope.cloudapp.azure.com/buy/api/buy');
    sleep(1);
}
