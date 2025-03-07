/*
opengl教程页面:
1.CN
https://learnopengl-cn.github.io/01%20Getting%20started/02%20Creating%20a%20window/
2.EN
https://learnopengl.com/Getting-started/OpenGL

踩坑:
1.#include <GLFW/glfw3.h>
https://blog.csdn.net/HANZY72/article/details/108656749
2.#include <glad/glad.h>
https://blog.csdn.net/u012278016/article/details/105582080
include顺序是有要求的.

报错:
1.#error : OpenGL header already included,remove this include, glad already provides it
https://blog.csdn.net/qq_40079310/article/details/114966609
*/

#include <glad/glad.h> 
#include <GLFW/glfw3.h>

#include <iostream>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow* window);

// 顶点着色器
const char* vertexShaderSource =
"#version 330 core\n"
"layout(location = 0) in vec3 aPos;\n"
"layout(location = 1) in vec3 aColor;\n"	// 颜色变量的属性位置值为1
"out vec3 ourColor;\n"

"void main()\n"
"{\n"
"	gl_Position = vec4(aPos, 1);\n"
"	ourColor = aColor;\n"	// 将ourColor设置为我们从顶点数据那里得到的输入颜色
"}\0";

// 片段着色器
const char* fragmentShaderSource =
"#version 330 core\n"
"out vec4 FragColor;\n"
"in vec3 ourColor;\n"	// 从顶点着色器传来的输入变量(名称相同,类型相同)

"void main(){\n"
"	FragColor = vec4(ourColor, 1);\n"
"}\0";

int main1(void)
{
	std::cout << "Hello World! OpenGL!" << std::endl;

	glfwInit();	//初始化glfw
	//glfwWindowHint()配置GLFW,第一个参数是枚举,第二个参数是在 https://www.glfw.org/docs/latest/window.html#window_hints 里面查找
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);	// MAJOR,主版本号为3
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);	// MINOR,次版本号为3
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);	// 告诉GLFW,使用核心模式,CORE

	GLFWwindow* window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL);	// 创建一个窗口对象,传入宽高,窗口标题
	if (window == NULL)
	{
		std::cout << "Failed to create GLFW window" << std::endl;
		glfwTerminate();
		return -1;
	}
	glfwMakeContextCurrent(window);
	glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);	// 把回调注册进glfw的回调函数里面

	//GLAD是用来管理OpenGL的函数指针的,所以在调用任何OpenGL的函数之前我们需要初始化GLAD
	if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
	{
		std::cout << "Failed to initialize GLAD" << std::endl;
		return -1;
	}

	// 编译顶点着色器
	unsigned int vertexShader = glCreateShader(GL_VERTEX_SHADER);	// 创建着色器，存在vertexShader里面
	glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
	glCompileShader(vertexShader);	// 编译

	// 判断是否编译成功
	int success;
	char infoLog[512];
	glGetShaderiv(vertexShader, GL_COMPILE_STATUS, &success);

	if (!success) {
		glGetShaderInfoLog(vertexShader, 512, NULL, infoLog);	// glGetShaderInfoLog()用来获取错误消息
		std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
	}

	// 编译片元着色器
	unsigned int fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
	glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
	glCompileShader(fragmentShader);

	// 判断是否编译成功
	glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, &success);
	if (!success) {
		glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
		std::cout << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << infoLog << std::endl;
	}

	// 把两个着色器对象链接到一个用来渲染的着色器程序中(Shader Program)
	unsigned int shaderProgram = glCreateProgram();
	glAttachShader(shaderProgram, vertexShader);	// 把着色器附着到程序上面
	glAttachShader(shaderProgram, fragmentShader);
	glLinkProgram(shaderProgram);	// 用glLinkProgram()链接着色器

	// 校验是否已经链接成功
	glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
	if (!success) {
		glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
		std::cout << "ERROR:PROGRAM::LINK_FAILED\n" << infoLog << std::endl;
	}

	// 链接成功之后,要删除着色器对象,因为已经不需要他们了
	glDeleteShader(vertexShader);
	glDeleteShader(fragmentShader);

	// 这一部分是绘制部分
	// 每三个位置组成一个三角形
	float vertices[] = {
		-0.5f, -0.5f, 0.0f, 1,0,0,
		0.5f, -0.5f, 0.0f,	0,1,0,
		0.0f, 0.5f, 0.0f,	0,0,1,
	};

	// 顶点数组对象
	unsigned int VBO, VAO;
	glGenVertexArrays(1, &VAO); // 绑定VAO
	glGenBuffers(1, &VBO);

	glBindVertexArray(VAO);
	glBindBuffer(GL_ARRAY_BUFFER, VBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	// 链接顶点属性
	// 参数一:顶点属性,对应顶点着色器中的layout(location=0)
	// 参数二:顶点属性的大小,是一个vec3,它由3个值组成,所以大小是3
	// 参数三:参数指定数据的类型,这里是GL_FLOAT(GLSL中vec*都是由浮点数值组成的)
	// 是否希望数据被标准化(Normalize),TRUE:所有数据都会被映射到0~1之间
	// 步长
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(0);

	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(1);

	// 激活这个程序对象,之后每个着色器调用和渲染都会使用这个程序对象
	glUseProgram(shaderProgram);
	//glBindBuffer(GL_ARRAY_BUFFER, 0);

	// 添加一个渲染循环
	while (!glfwWindowShouldClose(window))	//glfwWindowShouldClose()函数在我们每次循环的开始前检查一次GLFW是否被要求退出,如果是的话返回true并且结束渲染循环,之后我们就可以关闭应用程序了
	{
		//input
		processInput(window);

		// 渲染
		// 清除颜色缓冲
		glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT);

		// glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);	// 开启线框模式
		// glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);	// 关闭线框模式

		// 激活着色器
		//glUseProgram(shaderProgram);

		// 颜色跟随时间变动
		//float timeValue = glfwGetTime();
		//float greenValue = (sin(timeValue) / 2) + 0.5f;
		//int vertexColorLocation = glGetUniformLocation(shaderProgram, "outColor");
		//glUniform4f(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

		// 绘制三级形
		glBindVertexArray(VAO);
		glDrawArrays(GL_TRIANGLES, 0, 6);

		glfwSwapBuffers(window); //glfwPollEvents()函数检查有没有触发什么事件,(比如键盘输入,鼠标移动等),更新窗口状态,并调用对应的回调函数(可以通过回调方法手动设置)
		glfwPollEvents();		//glfwSwapBuffers()函数会交换颜色缓冲(它是一个储存着GLFW窗口每一个像素颜色值的大缓冲),它在这一迭代中被用来绘制,并且将会作为输出显示在屏幕上。
	}

	//双缓冲(Double Buffer),避免出现图像闪烁的问题

	//释放/删除之前的分配的所有资源
	glfwTerminate();
	return 0;
}

void processInput(GLFWwindow* window) {
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS) {
		glfwSetWindowShouldClose(window, true);
	}
}

// 回调,当窗口大小被改变的时候,重新设置OpenGL渲染的大小
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
	glViewport(0, 0, width, height);
}
