﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using TcpCommunication.Classes.Services;
using TcpCommunication.Interfaces;

namespace TcpCommunication.Classes.Messages
{
    [DataContract]
    public class LoginMessage : XmlStorage<LoginMessage>, IMessage
    {
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public Response Response { get; private set; }

        public LoginMessage() : base()
        {
            Login = string.Empty;
            Response = null;
        }
        public override bool InitializeFromObject(LoginMessage Object)
        {
            Login = Object.Login;
            Response = Object.Response;

            return true;
        }

        public IMessage ProcessRequest(StateObject Object = null)
        {
            var _client = Object.GetObject<ClientService>();

            if (_client.IsHostedBy)
            {
                var _server = _client.GetHostedBy<ServerService<ClientService>>();

                if (_server.ConnectedClients.Find(x => x.Identifier == Login) == null)
                {
                    _client.Identifier = Login;
                    Response = new Response(1, "Zalogowano poprawne");
                }
                else
                {
                    Response = new Response(0, "Uzytkownik o takim loginie juz zalogowany!");
                }
            }
            else
                Response = new Response(0, new Exception("Wyjątek krytyczny"));

            return this;
        }

        public IMessage ProcessResponse(StateObject Object = null)
        {
            var _client = Object.GetObject<ClientService>();

            if (Response.ResponseObject is Exception)
                throw Response.ResponseObject as Exception;

            if (Response.ResponseCode == 0)
                throw new Exception($"Błąd podczas logowania! {Response}");

            if (Response.ResponseCode == 1)
            {
                _client.Identifier = Login;
            }

            return this;                   
        }

        public override string ToString()
        {
            return $"[Login={Login}|Response={Response}]";
        }
    }
}
