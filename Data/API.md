###Consuming the iraid API.

---

<br>

> *Keep in mind that changes are prone to happen, API is still under heavy development.*

<br>


All communication with the iraid backend service happens by sending and receiving json formatted messages sent via websocket.

*Available services:*  

 * **ws://localhost:8888/Web**  
 * **ws://localhost:8888/Bot**

See the respective API documentations for available methods.

<br>  
If you want to write a application that consumes the iraid API's, conform your message as follow.

<pre>
{
    "action": "Method that you want to Invoke",
    "params": {
        "FirstParameter": "value",
        "SecondParameter": "value"
    }
}
</pre>

<br>  
Answers from server will always conform the following schema.

<pre>
{
    "action": "An action that you previously tried to invoke, existant or not.",
    "success": "true or false",
    "reply": "Exception message if success equals false, otherwise a json formatted message with the methods corrosponding return type.",
    "time": "DateTime when execution took place.",
    "executiontime": "Time in milliseconds server used to process your request."
}
</pre>

<br>  
Example of authentication.

<pre>
{
    "action": "Auth",
    "params": {
        "username": "Demo",
        "password": "3434JDSKJS323JDDJ",
    }
}
</pre>


<br>  

> *Tip **Almost all methods are conformed so that which you can just return the counterpart object you received from server.***