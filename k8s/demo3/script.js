import http from 'k6/http';
import { sleep } from 'k6';

export default function () {
    http.post('http://fantasticbike.westeurope.cloudapp.azure.com/api/buy');
    sleep(1);
}
