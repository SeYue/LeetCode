/*
opengl�̳�ҳ��:
1.CN
https://learnopengl-cn.github.io/01%20Getting%20started/02%20Creating%20a%20window/
2.EN
https://learnopengl.com/Getting-started/OpenGL

�ȿ�:
1.#include <GLFW/glfw3.h>
https://blog.csdn.net/HANZY72/article/details/108656749
2.#include <glad/glad.h>
https://blog.csdn.net/u012278016/article/details/105582080
include˳������Ҫ���.

����:
1.#error : OpenGL header already included,remove this include, glad already provides it
https://blog.csdn.net/qq_40079310/article/details/114966609
*/

#include <glad/glad.h> 
#include <GLFW/glfw3.h>

#include <iostream>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow* window);

// ������ɫ��
const char* vertexShaderSource =
"#version 330 core\n"
"layout(location = 0) in vec3 aPos;\n"
"layout(location = 1) in vec3 aColor;\n"	// ��ɫ����������λ��ֵΪ1
"out vec3 ourColor;\n"

"void main()\n"
"{\n"
"	gl_Position = vec4(aPos, 1);\n"
"	ourColor = aColor;\n"	// ��ourColor����Ϊ���ǴӶ�����������õ���������ɫ
"}\0";

// Ƭ����ɫ��
const char* fragmentShaderSource =
"#version 330 core\n"
"out vec4 FragColor;\n"
"in vec3 ourColor;\n"	// �Ӷ�����ɫ���������������(������ͬ,������ͬ)

"void main(){\n"
"	FragColor = vec4(ourColor, 1);\n"
"}\0";

int main1(void)
{
	std::cout << "Hello World! OpenGL!" << std::endl;

	glfwInit();	//��ʼ��glfw
	//glfwWindowHint()����GLFW,��һ��������ö��,�ڶ����������� https://www.glfw.org/docs/latest/window.html#window_hints �������
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);	// MAJOR,���汾��Ϊ3
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);	// MINOR,�ΰ汾��Ϊ3
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);	// ����GLFW,ʹ�ú���ģʽ,CORE

	GLFWwindow* window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL);	// ����һ�����ڶ���,������,���ڱ���
	if (window == NULL)
	{
		std::cout << "Failed to create GLFW window" << std::endl;
		glfwTerminate();
		return -1;
	}
	glfwMakeContextCurrent(window);
	glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);	// �ѻص�ע���glfw�Ļص���������

	//GLAD����������OpenGL�ĺ���ָ���,�����ڵ����κ�OpenGL�ĺ���֮ǰ������Ҫ��ʼ��GLAD
	if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
	{
		std::cout << "Failed to initialize GLAD" << std::endl;
		return -1;
	}

	// ���붥����ɫ��
	unsigned int vertexShader = glCreateShader(GL_VERTEX_SHADER);	// ������ɫ��������vertexShader����
	glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
	glCompileShader(vertexShader);	// ����

	// �ж��Ƿ����ɹ�
	int success;
	char infoLog[512];
	glGetShaderiv(vertexShader, GL_COMPILE_STATUS, &success);

	if (!success) {
		glGetShaderInfoLog(vertexShader, 512, NULL, infoLog);	// glGetShaderInfoLog()������ȡ������Ϣ
		std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
	}

	// ����ƬԪ��ɫ��
	unsigned int fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
	glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
	glCompileShader(fragmentShader);

	// �ж��Ƿ����ɹ�
	glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, &success);
	if (!success) {
		glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
		std::cout << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << infoLog << std::endl;
	}

	// ��������ɫ���������ӵ�һ��������Ⱦ����ɫ��������(Shader Program)
	unsigned int shaderProgram = glCreateProgram();
	glAttachShader(shaderProgram, vertexShader);	// ����ɫ�����ŵ���������
	glAttachShader(shaderProgram, fragmentShader);
	glLinkProgram(shaderProgram);	// ��glLinkProgram()������ɫ��

	// У���Ƿ��Ѿ����ӳɹ�
	glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
	if (!success) {
		glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
		std::cout << "ERROR:PROGRAM::LINK_FAILED\n" << infoLog << std::endl;
	}

	// ���ӳɹ�֮��,Ҫɾ����ɫ������,��Ϊ�Ѿ�����Ҫ������
	glDeleteShader(vertexShader);
	glDeleteShader(fragmentShader);

	// ��һ�����ǻ��Ʋ���
	// ÿ����λ�����һ��������
	float vertices[] = {
		-0.5f, -0.5f, 0.0f, 1,0,0,
		0.5f, -0.5f, 0.0f,	0,1,0,
		0.0f, 0.5f, 0.0f,	0,0,1,
	};

	// �����������
	unsigned int VBO, VAO;
	glGenVertexArrays(1, &VAO); // ��VAO
	glGenBuffers(1, &VBO);

	glBindVertexArray(VAO);
	glBindBuffer(GL_ARRAY_BUFFER, VBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	// ���Ӷ�������
	// ����һ:��������,��Ӧ������ɫ���е�layout(location=0)
	// ������:�������ԵĴ�С,��һ��vec3,����3��ֵ���,���Դ�С��3
	// ������:����ָ�����ݵ�����,������GL_FLOAT(GLSL��vec*�����ɸ�����ֵ��ɵ�)
	// �Ƿ�ϣ�����ݱ���׼��(Normalize),TRUE:�������ݶ��ᱻӳ�䵽0~1֮��
	// ����
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(0);

	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(1);

	// ��������������,֮��ÿ����ɫ�����ú���Ⱦ����ʹ������������
	glUseProgram(shaderProgram);
	//glBindBuffer(GL_ARRAY_BUFFER, 0);

	// ���һ����Ⱦѭ��
	while (!glfwWindowShouldClose(window))	//glfwWindowShouldClose()����������ÿ��ѭ���Ŀ�ʼǰ���һ��GLFW�Ƿ�Ҫ���˳�,����ǵĻ�����true���ҽ�����Ⱦѭ��,֮�����ǾͿ��Թر�Ӧ�ó�����
	{
		//input
		processInput(window);

		// ��Ⱦ
		// �����ɫ����
		glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT);

		// glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);	// �����߿�ģʽ
		// glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);	// �ر��߿�ģʽ

		// ������ɫ��
		//glUseProgram(shaderProgram);

		// ��ɫ����ʱ��䶯
		//float timeValue = glfwGetTime();
		//float greenValue = (sin(timeValue) / 2) + 0.5f;
		//int vertexColorLocation = glGetUniformLocation(shaderProgram, "outColor");
		//glUniform4f(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

		// ����������
		glBindVertexArray(VAO);
		glDrawArrays(GL_TRIANGLES, 0, 6);

		glfwSwapBuffers(window); //glfwPollEvents()���������û�д���ʲô�¼�,(�����������,����ƶ���),���´���״̬,�����ö�Ӧ�Ļص�����(����ͨ���ص������ֶ�����)
		glfwPollEvents();		//glfwSwapBuffers()�����ύ����ɫ����(����һ��������GLFW����ÿһ��������ɫֵ�Ĵ󻺳�),������һ�����б���������,���ҽ�����Ϊ�����ʾ����Ļ�ϡ�
	}

	//˫����(Double Buffer),�������ͼ����˸������

	//�ͷ�/ɾ��֮ǰ�ķ����������Դ
	glfwTerminate();
	return 0;
}

void processInput(GLFWwindow* window) {
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS) {
		glfwSetWindowShouldClose(window, true);
	}
}

// �ص�,�����ڴ�С���ı��ʱ��,��������OpenGL��Ⱦ�Ĵ�С
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
	glViewport(0, 0, width, height);
}
