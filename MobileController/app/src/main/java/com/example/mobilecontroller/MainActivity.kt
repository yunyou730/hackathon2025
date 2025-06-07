package com.example.mobilecontroller

import android.app.Activity
import android.content.Context
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import android.os.Bundle
import android.util.Log
import android.widget.Toast
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
import kotlinx.coroutines.*
import java.io.IOException
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress

class MainActivity : ComponentActivity(), SensorEventListener {

    private lateinit var sensorManager: SensorManager
    private lateinit var gyroscopeSensor: Sensor

    private val _gyroData = mutableStateOf("X: 0.00\nY: 0.00\nZ: 0.00")
    val gyroData: State<String> get() = _gyroData

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MobileControllerTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    UdpClientScreen(gyroData = gyroData)
                }
            }
        }
        sensorManager = getSystemService(Context.SENSOR_SERVICE) as SensorManager
        gyroscopeSensor = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE) ?: run {
            Toast.makeText(this, "设备不支持陀螺仪", Toast.LENGTH_SHORT).show()
            return
        }
    }

    override fun onResume() {
        super.onResume()
        sensorManager.registerListener(this, gyroscopeSensor, SensorManager.SENSOR_DELAY_GAME)
    }

    override fun onPause() {
        super.onPause()
        sensorManager.unregisterListener(this)
    }

    override fun onSensorChanged(event: SensorEvent) {
        if (event.sensor.type == Sensor.TYPE_GYROSCOPE) {
            val x = event.values[0]
            val y = event.values[1]
            val z = event.values[2]

            runOnUiThread {
                _gyroData.value = "X: ${String.format("%.2f", x)} rad/s\nY: ${String.format("%.2f", y)} rad/s\nZ: ${String.format("%.2f", z)} rad/s"
            }
        }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {
        // 不需要实现
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun UdpClientScreen(gyroData: State<String>) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope() // 创建 CoroutineScope
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

    // 设置状态栏和导航栏颜色
    DisposableEffect(view) {
        if (!view.isInEditMode) {
            val window = (context as? Activity)?.window
                ?: throw IllegalStateException("无法获取Activity窗口")

            WindowCompat.setDecorFitsSystemWindows(window, false)
            window.statusBarColor = statusBarColor.hashCode()
            window.navigationBarColor = navigationBarColor.hashCode()
        }

        onDispose {
            // 清理操作
            disconnect(socket, receiverJob, isConnected)
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
                                messages.add("已断开连接")
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
                                            messages = messages,
                                            scope = scope // 传递 CoroutineScope
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
                    singleLine = true
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
            Text(
                text = "陀螺仪数据:",
                color = MaterialTheme.colorScheme.primary,
                fontWeight = FontWeight.Bold,
                fontSize = 14.sp
            )

            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(120.dp)
                    .padding(vertical = 8.dp),
                shape = RoundedCornerShape(8.dp),
                elevation = CardDefaults.cardElevation(4.dp)
            ) {
                Text(
                    text = gyroData.value,
                    modifier = Modifier.padding(16.dp),
                    fontSize = 14.sp,
                    color = MaterialTheme.colorScheme.onSurface
                )
            }

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

// 连接到UDP服务器（修改：添加 scope 参数）
private suspend fun connect(
    ip: String,
    port: Int,
    socket: MutableState<DatagramSocket?>,
    receiverJob: MutableState<kotlinx.coroutines.Job?>,
    isConnected: MutableState<Boolean>,
    messages: MutableList<String>,
    scope: CoroutineScope // 新增参数
) = withContext(Dispatchers.IO) {
    try {
        // 创建UDP套接字
        socket.value = DatagramSocket()
        isConnected.value = true

        // 使用传递的 scope 启动协程（在IO线程接收数据，在主线程更新UI）
        receiverJob.value = scope.launch(Dispatchers.IO) {
            val buffer = ByteArray(1024)
            while (isConnected.value) {
                try {
                    val packet = DatagramPacket(buffer, buffer.size)
                    socket.value?.receive(packet)
                    val message = String(packet.data, 0, packet.length)

                    // 切换到主线程更新UI
                    withContext(Dispatchers.Main) {
                        messages.add("收到: $message")
                    }
                } catch (e: IOException) {
                    if (isConnected.value) {
                        withContext(Dispatchers.Main) {
                            messages.add("接收消息出错: ${e.message}")
                            disconnect(socket, receiverJob, isConnected)
                        }
                    }
                    break
                }
            }
        }
    } catch (e: Exception) {
        withContext(Dispatchers.Main) {
            messages.add("连接错误: ${e.message}")
            disconnect(socket, receiverJob, isConnected)
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
        // 创建一个临时的gyroData状态用于预览
        val tempGyroData = remember { mutableStateOf("X: 0.00\nY: 0.00\nZ: 0.00") }
        UdpClientScreen(gyroData = tempGyroData)
    }
}