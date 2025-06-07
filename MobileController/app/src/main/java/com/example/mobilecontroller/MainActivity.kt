package com.example.mobilecontroller

import android.app.Activity
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.TextFieldValue
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.view.WindowCompat
import com.example.mobilecontroller.ui.theme.MobileControllerTheme
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.IOException
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress

class MainActivity : ComponentActivity(), SensorEventListener {



    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MobileControllerTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    UdpClientScreen()
                }
            }
        }


    }

    override fun onSensorChanged(event: SensorEvent?) {
        TODO("Not yet implemented")
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {
        TODO("Not yet implemented")
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun UdpClientScreen() {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val view = LocalView.current

    // 状态管理
    val ipAddress = remember { mutableStateOf(TextFieldValue("127.0.0.1")) }
    val port = remember { mutableStateOf(TextFieldValue("8888")) }
    val message = remember { mutableStateOf(TextFieldValue("")) }
    val messages = remember { mutableListOf<String>() }
    val isConnected = remember { mutableStateOf(false) }
    val socket = remember { mutableStateOf<DatagramSocket?>(null) }
    val receiverJob = remember { mutableStateOf<kotlinx.coroutines.Job?>(null) }

    // 状态栏颜色
    val statusBarColor = MaterialTheme.colorScheme.primaryContainer
    val navigationBarColor = MaterialTheme.colorScheme.surface

    // 设置状态栏和导航栏颜色（修复后的代码）
    DisposableEffect(view) {
        if (!view.isInEditMode) {
            val window = (context as? Activity)?.window
                ?: throw IllegalStateException("无法获取Activity窗口")

            WindowCompat.setDecorFitsSystemWindows(window, false)
            window.statusBarColor = statusBarColor.hashCode()
            window.navigationBarColor = navigationBarColor.hashCode()
        }

        onDispose {
            // 清理操作（可选）
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("UDP 控制器") },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        },
        bottomBar = {
            BottomAppBar(
                containerColor = MaterialTheme.colorScheme.surfaceVariant,
                contentColor = MaterialTheme.colorScheme.onSurfaceVariant
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Button(
                        onClick = {
                            if (isConnected.value) {
                                // 断开连接
                                disconnect(socket, receiverJob, isConnected)
                            } else {
                                // 连接
                                scope.launch {
                                    try {
                                        val portNum = port.value.text.toIntOrNull() ?: 8888
                                        connect(
                                            ip = ipAddress.value.text,
                                            port = portNum,
                                            socket = socket,
                                            receiverJob = receiverJob,
                                            isConnected = isConnected,
                                            messages = messages
                                        )
                                        messages.add("已连接到 ${ipAddress.value.text}:$portNum")
                                    } catch (e: Exception) {
                                        messages.add("连接失败: ${e.message}")
                                    }
                                }
                            }
                        },
                        modifier = Modifier.weight(1f),
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (isConnected.value)
                                MaterialTheme.colorScheme.errorContainer
                            else
                                MaterialTheme.colorScheme.primaryContainer,
                            contentColor = if (isConnected.value)
                                MaterialTheme.colorScheme.onErrorContainer
                            else
                                MaterialTheme.colorScheme.onPrimaryContainer
                        )
                    ) {
                        Text(if (isConnected.value) "断开连接" else "连接")
                    }

                    Spacer(modifier = Modifier.width(8.dp))

                    Button(
                        onClick = {
                            if (isConnected.value && message.value.text.isNotBlank()) {
                                scope.launch {
                                    sendMessage(
                                        message = message.value.text,
                                        socket = socket.value,
                                        ip = ipAddress.value.text,
                                        port = port.value.text.toIntOrNull() ?: 8888,
                                        messages = messages
                                    )
                                    message.value = TextFieldValue("")
                                }
                            }
                        },
                        enabled = isConnected.value && message.value.text.isNotBlank(),
                        colors = ButtonDefaults.buttonColors(
                            containerColor = MaterialTheme.colorScheme.secondaryContainer,
                            contentColor = MaterialTheme.colorScheme.onSecondaryContainer
                        )
                    ) {
                        Icon(Icons.Default.Send, contentDescription = "发送消息")
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("发送")
                    }
                }
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp)
        ) {
            // 连接信息
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                OutlinedTextField(
                    value = ipAddress.value,
                    onValueChange = { ipAddress.value = it },
                    label = { Text("IP 地址") },
                    modifier = Modifier.weight(3f),
                    singleLine = true
                )

                Spacer(modifier = Modifier.width(8.dp))

                OutlinedTextField(
                    value = port.value,
                    onValueChange = { port.value = it },
                    label = { Text("端口") },
                    modifier = Modifier.weight(1f),
                    singleLine = true,


                    //keyboardOptions = androidx.compose.ui.text.input.KeyboardOptions(
                    //    keyboardType = androidx.compose.ui.text.input.KeyboardType.Number
                    //)
                )
            }

            Spacer(modifier = Modifier.height(16.dp))

            // 消息输入框
            OutlinedTextField(
                value = message.value,
                onValueChange = { message.value = it },
                label = { Text("输入消息") },
                modifier = Modifier.fillMaxWidth(),
                minLines = 3,
                maxLines = 5
            )

            Spacer(modifier = Modifier.height(16.dp))

            // 连接状态
            Text(
                text = "连接状态: ${if (isConnected.value) "已连接" else "未连接"}",
                color = if (isConnected.value) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.error,
                fontWeight = FontWeight.Bold,
                fontSize = 14.sp
            )

            Spacer(modifier = Modifier.height(8.dp))

            // 消息显示区域
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(1f),
                shape = RoundedCornerShape(8.dp),
                elevation = CardDefaults.cardElevation(4.dp)
            ) {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(8.dp),
                    reverseLayout = false,
                    verticalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    items(messages) { msg ->
                        Text(msg)
                    }
                }
            }
        }
    }
}

// 连接到UDP服务器
private suspend fun connect(
    ip: String,
    port: Int,
    socket: MutableState<DatagramSocket?>,
    receiverJob: MutableState<kotlinx.coroutines.Job?>,
    isConnected: MutableState<Boolean>,
    messages: MutableList<String>
) = withContext(Dispatchers.IO) {
    try {
        // 创建UDP套接字
        socket.value = DatagramSocket()
        isConnected.value = true

        // 启动接收消息的协程
        receiverJob.value = launch {
            val buffer = ByteArray(1024)
            while (isConnected.value) {
                try {
                    val packet = DatagramPacket(buffer, buffer.size)
                    socket.value?.receive(packet)
                    val message = String(packet.data, 0, packet.length)

                    withContext(Dispatchers.Main) {
                        messages.add("收到: $message")
                    }
                } catch (e: IOException) {
                    if (isConnected.value) {
                        withContext(Dispatchers.Main) {
                            messages.add("接收消息出错: ${e.message}")
                            isConnected.value = false
                        }
                    }
                    break
                }
            }
        }
    } catch (e: Exception) {
        withContext(Dispatchers.Main) {
            messages.add("连接错误: ${e.message}")
            isConnected.value = false
        }
        throw e
    }
}

// 发送消息
private suspend fun sendMessage(
    message: String,
    socket: DatagramSocket?,
    ip: String,
    port: Int,
    messages: MutableList<String>
) = withContext(Dispatchers.IO) {
    try {
        socket?.let {
            val data = message.toByteArray()
            val packet = DatagramPacket(data, data.size, InetAddress.getByName(ip), port)
            it.send(packet)

            withContext(Dispatchers.Main) {
                messages.add("发送: $message")
            }
        }
    } catch (e: Exception) {
        withContext(Dispatchers.Main) {
            messages.add("发送失败: ${e.message}")
        }
    }
}

// 断开连接
private fun disconnect(
    socket: MutableState<DatagramSocket?>,
    receiverJob: MutableState<kotlinx.coroutines.Job?>,
    isConnected: MutableState<Boolean>
) {
    receiverJob.value?.cancel()
    socket.value?.close()
    socket.value = null
    isConnected.value = false
}

@Preview(showBackground = true)
@Composable
fun DefaultPreview() {
    MobileControllerTheme {
        UdpClientScreen()
    }
}