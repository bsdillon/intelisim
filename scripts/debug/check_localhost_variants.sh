getent hosts localhost
getent hosts 127.0.0.1


curl -v http://localhost:5000/socket.io/?transport=polling&EIO=4
curl -v http://127.0.0.1:5000/socket.io/?transport=polling&EIO=4
