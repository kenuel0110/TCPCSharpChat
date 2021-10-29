import threading
import socket
import signal
import os


host = "127.0.0.1"
port = 55555

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((host, port))
server.listen()

clients = []        #ip "клиентов"


def broadcast(message): #Функция отправки
    for client in clients:
        client.send(message)

def handle(client):     #"слушатель"
    while True:
        try:
            message = client.recv(1024)
            broadcast(message)
        except:
            index = clients.index(client)
            clients.remove(client)
            client.close()
            break

def recieve():
    while True:
        client, address = server.accept()
        print(f'Соеденён с адрессом: {str(address)}')

        clients.append(client)

        thread = threading.Thread(target=handle, args=(client,))    #добавление потока для handle
        thread.start()

print("На связи")

recieve()
