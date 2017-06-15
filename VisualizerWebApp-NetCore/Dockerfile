FROM microsoft/aspnetcore:1.1

ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .

EXPOSE 2222 80

RUN apt-get update && apt-get install -y graphviz \
	&& apt-get install -y openssh-server dos2unix --no-install-recommends \
    && echo "root:Docker!" | chpasswd 

COPY init_container.sh /bin/
COPY sshd_config /etc/ssh/

RUN dos2unix /etc/ssh/sshd_config && dos2unix /bin/init_container.sh 
CMD ["/bin/init_container.sh"]

