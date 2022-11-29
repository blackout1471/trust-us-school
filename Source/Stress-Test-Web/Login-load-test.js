import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
    stages: [ // Duration = time, target = clients
        {duration: '5s', target: 300}, // Normal load
        {duration: '30s', target: 300}, // Normal load
        {duration: '5s', target: 1000}, // High load
        {duration: '1m', target: 1000}, // High load
        {duration: '3m', target: 100} // Recovery load
    ],
    thresholds: {
        'http_req_duration': ['p(99)<1500'] // 99% of requests finish in 1.5s
    },
};

const PORT = 5000;
const BASE_URL = 'http://host.docker.internal'

export default function () {
    const url = BASE_URL + ':' + PORT + '/api/user/login';
    const payload = JSON.stringify({
      email: 'string@asd.dk',
      password: 'stringst',
    });
  
    const params = {
      headers: {
        'Content-Type': 'application/json',
      },
    };

    http.post(url, payload, params);
    sleep(1); // sleep for 1 sec, so 1 user makes 1 per second
}