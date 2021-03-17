using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        struct Inimigo
        {
            public double posX;
            public double posY;
            public double novposX;
            public double novposY;
            public DirecoesCardeais direcaoMovimento;
            public double espera;
        }

        struct Cartesiano
        {
            public int x;
            public int y;
        }

        enum DirecoesCardeais
        {
            nulo = 0,
            norte = 1,
            leste = 2,
            sul = 3,
            oeste = 4
        }

        static int inimigo0score = 100;

        const int largTela = 35;
        const int altuTela = 22;
        static int explosaoRaio = 1;
        static int c1dPowerUp = 0;
        static int c1dSaida = 0;
        static int nivelAtual = 0;
        static double jogadorX = 15;
        static double jogadorY = 10;
        static double newJogadorX = 15;
        static double newJogadorY = 10;
        static string background; 
        const string solidos = "█▓Ó☺";
        static bool gameOver = false;
        static bool input;        
        static List<int> coordenadas1dExplosao = new List<int>();
        static List<int> coordenadas1dDestruicao = new List<int>();
        static Queue<Inimigo> listaInimigos = new Queue<Inimigo>();
        static Random random = new Random();

        [STAThread]
        static void Main(string[] args)
        {
            
            const int bombaTrigger = 3;            
            int score = 0;
            int bombaX = 0;
            int bombaY = 0;
            double tempoJogo = 180;
            double tempoSobrando = 0;
            double fatorTempo;
            DateTime inicioJogo;
            DateTime tempoAtual;
            DateTime tempoAnterior;
            DateTime bombaTimer = DateTime.Now;
            DateTime explosaoInicio = DateTime.Now;
            bool bombaDesenha = false;
            bool bombaExiste = false;
            bool explosaoSome = false;
            bool bombaInput = false;
            bool explosao = false;
            bool proximoNivel = false;
            bool powerUpExiste = true;

            Console.CursorVisible = false;

            Console.SetWindowSize(largTela, altuTela);

            GeraNivel();

            tempoAnterior = inicioJogo = DateTime.Now; 

            while (!gameOver)
            {
                if (proximoNivel)
                {
                    GeraNivel();
                    inicioJogo = DateTime.Now;
                    proximoNivel = false;
                }
                
                //Fator de Tempo
                tempoAtual = DateTime.Now;
                fatorTempo = (tempoAtual - tempoAnterior).TotalSeconds;

                //Inputs
                input = false;

                if (Keyboard.IsKeyDown(Key.W)) 
                {
                    newJogadorY = jogadorY - (12 * fatorTempo);                                           //Faz o movimento
                    input = true;
                    input = Colisão(ref newJogadorX, ref newJogadorY, jogadorY, 'y');                  //Se houver colisão, desfaz.                    
                }
                if (Keyboard.IsKeyDown(Key.S))
                {
                    newJogadorY = jogadorY + (12 * fatorTempo);                                           
                    input = true;
                    input = Colisão(ref newJogadorX, ref newJogadorY, jogadorY, 'y');
                }
                if (Keyboard.IsKeyDown(Key.A))
                {
                    newJogadorX = jogadorX - (16 * fatorTempo);                                           
                    input = true;
                    input = Colisão(ref newJogadorX, ref newJogadorY, jogadorX, 'x');                    
                }
                if (Keyboard.IsKeyDown(Key.D))
                {
                    newJogadorX = jogadorX + (16 * fatorTempo);                                           
                    input = true;
                    input = Colisão(ref newJogadorX, ref newJogadorY, jogadorX, 'x');
                }
                if (Keyboard.IsKeyDown(Key.NumPad0) && !bombaExiste)
                {
                    if (bombaInput == false)
                    {
                        bombaExiste = true;
                        bombaDesenha = true;
                        bombaInput = true;
                        bombaTimer = DateTime.Now;
                        bombaX = (int)newJogadorX;
                        bombaY = (int)newJogadorY;
                    }
                }
                else
                {
                    bombaInput = false;
                }

                //Funcionamento
                if (bombaExiste && (DateTime.Now - bombaTimer).TotalSeconds >= bombaTrigger)
                {
                    if (!explosao)
                    {
                        explosaoInicio = DateTime.Now;
                        explosao = true;
                        ExecutaExplosao(bombaX, bombaY);
                    }
                                        
                    if ((int)newJogadorX == bombaX && (int)newJogadorY == bombaY)
                    {
                        gameOver = true;
                    }

                    if((DateTime.Now - explosaoInicio).TotalSeconds >= 0.5)
                    {
                        explosao = false;
                        bombaExiste = false;
                        explosaoSome = true;
                    }                                       
                }

                if (Coordenada1D((int)newJogadorX, (int)newJogadorY) == c1dPowerUp && powerUpExiste)
                {
                    explosaoRaio++;
                    powerUpExiste = false;
                }

                if (Coordenada1D((int)newJogadorX, (int)newJogadorY) == c1dSaida)
                {
                    proximoNivel = true;
                }

                tempoSobrando = tempoJogo - (DateTime.Now - inicioJogo).TotalSeconds;

                if(tempoSobrando <= 0)
                {
                    gameOver = true;
                }

                //Inimigos

                int numeroInimigos = listaInimigos.Count();
                for (int i = 0; i < numeroInimigos; i++)
                {
                    Inimigo inimigo = listaInimigos.Dequeue();
                    inimigo.posX = inimigo.novposX;
                    inimigo.posY = inimigo.novposY;
                    if (inimigo.espera > 0)
                    {
                        inimigo.espera -= fatorTempo;
                    }
                    else
                    {
                        switch (inimigo.direcaoMovimento)
                        {
                            case DirecoesCardeais.nulo:
                                DirecoesCardeais r = (DirecoesCardeais)random.Next(1, 5);
                                inimigo.direcaoMovimento = r;
                                inimigo.espera = 0.5;
                                break;
                            case DirecoesCardeais.norte:
                                inimigo.novposY = inimigo.posY - (3 * fatorTempo);
                                if((int)inimigo.novposY != (int)inimigo.posY && !Colisão(ref inimigo.novposX, ref inimigo.novposY, inimigo.posY, 'y'))
                                {
                                    inimigo.direcaoMovimento = DirecoesCardeais.nulo;
                                }
                                break;
                            case DirecoesCardeais.sul:
                                inimigo.novposY = inimigo.posY + (3 * fatorTempo);
                                if ((int)inimigo.novposY != (int)inimigo.posY && !Colisão(ref inimigo.novposX, ref inimigo.novposY, inimigo.posY, 'y'))
                                {
                                    inimigo.direcaoMovimento = DirecoesCardeais.nulo;
                                }
                                break;
                            case DirecoesCardeais.oeste:
                                inimigo.novposX = inimigo.posX - (4 * fatorTempo);
                                if ((int)inimigo.novposX != (int)inimigo.posX && !Colisão(ref inimigo.novposX, ref inimigo.novposY, inimigo.posX, 'x'))
                                {
                                    inimigo.direcaoMovimento = DirecoesCardeais.nulo;
                                }
                                break;
                            case DirecoesCardeais.leste:
                                inimigo.novposX = inimigo.posX + (4 * fatorTempo);
                                if ((int)inimigo.novposX != (int)inimigo.posX && !Colisão(ref inimigo.novposX, ref inimigo.novposY, inimigo.posX, 'x'))
                                {
                                    inimigo.direcaoMovimento = DirecoesCardeais.nulo;
                                }
                                break;
                        }
                    }
                    listaInimigos.Enqueue(inimigo);
                }

                //Desenhar a Tela
                if (input && ((int)jogadorX != (int)newJogadorX) || ((int)jogadorY != (int)newJogadorY))
                {
                    background = background.Remove(Coordenada1D((int)jogadorX, (int)jogadorY), 1);
                    if (bombaDesenha)
                    {
                        background = background.Insert(Coordenada1D((int)jogadorX, (int)jogadorY), "Ó");
                        bombaDesenha = false;
                    }
                    else
                    {
                        background = background.Insert(Coordenada1D((int)jogadorX, (int)jogadorY), " ");
                    }
                    DesenhaCaracterePosicao((int)newJogadorX, (int)newJogadorY, '@');
                }

                int listaInimigosCount = listaInimigos.Count;
                for (int i = 0; i < listaInimigosCount; i++)
                {
                    bool enfileirar = true;
                    Inimigo inimigo = listaInimigos.Dequeue();
                    foreach (int coordenadaExplosao in coordenadas1dExplosao)
                    {
                        if (coordenadaExplosao == Coordenada1D((int)inimigo.novposX, (int)inimigo.novposY))
                        {
                            enfileirar = false;
                            score += inimigo0score;
                        }
                    }
                    if ((int)inimigo.posX != (int)inimigo.novposX || (int)inimigo.posY != (int)inimigo.novposY)
                    {
                        DesenhaCaracterePosicao((int)inimigo.posX, (int)inimigo.posY, ' ');
                        DesenhaCaracterePosicao((int)inimigo.novposX, (int)inimigo.novposY, '☺');
                    }
                    if ((int)newJogadorX == (int)inimigo.novposX && (int)newJogadorY == (int)inimigo.novposY)
                    {
                        gameOver = true;
                    }
                    
                    if (enfileirar)
                        listaInimigos.Enqueue(inimigo);
                }

                if (explosao)
                {
                    DesenhaCaracterePosicao(bombaX, bombaY, '☼');

                    foreach (int coordenadaExplosao in coordenadas1dExplosao)
                    {
                        DesenhaCaracterePosicao(coordenadaExplosao, '☼');
                    }

                    foreach (int coordenadaDestruicao in coordenadas1dDestruicao)
                    {
                        if (coordenadaDestruicao == c1dPowerUp)
                            DesenhaCaracterePosicao(coordenadaDestruicao, '®');
                        else if (coordenadaDestruicao == c1dSaida)
                            DesenhaCaracterePosicao(coordenadaDestruicao, '#');
                        else
                            DesenhaCaracterePosicao(coordenadaDestruicao, ' ');
                    }
                }

                if (explosaoSome)
                {
                    foreach (int coordenadaExplosao in coordenadas1dExplosao)
                    {
                        if (CaractereNaPosicao(coordenadaExplosao) == '☼')
                        {
                            DesenhaCaracterePosicao(coordenadaExplosao, ' ');
                        }
                    }
                    DesenhaCaracterePosicao(bombaX, bombaY, ' ');

                    explosaoSome = false;
                    coordenadas1dExplosao.Clear();
                    coordenadas1dDestruicao.Clear();
                        

                }

                if (gameOver)
                {
                    DesenhaCaracterePosicao((int)newJogadorX, (int)newJogadorY, 'X');
                }

                jogadorX = newJogadorX;
                jogadorY = newJogadorY;
                Console.SetCursorPosition(0, 0);
                Console.Write(background);
                Console.SetCursorPosition(0, 19);
                Console.WriteLine("Score: = " + score + " Tempo: = " + tempoSobrando.ToString("0") + " Nível: " + nivelAtual);
                if(listaInimigos.Count == 0)
                {
                    Cartesiano xysaida = CoordenadaXY(c1dSaida);
                    Console.Write("Saída = linha {0}, coluna {1}", xysaida.y, xysaida.x);
                }

                tempoAnterior = tempoAtual;
            }

            Console.SetCursorPosition((largTela / 2) - 6, altuTela / 2);
            Console.Write(" GAME OVER ");
            Console.SetCursorPosition(0, 19);
        }

        static int Coordenada1D(int x, int y)
        {
            return (y * largTela) + x;
        }

        static bool Colisão(ref double x, ref double y, double coordenadaAntiga, char tipoCoordenada)
        {
            bool caminhoLivre = true;
            foreach (char solido in solidos)
            {
                if (CaractereNaPosicao((int)x, (int)y) == solido) //Se houver colisão, desfaz.
                {
                    if (tipoCoordenada == 'x' || tipoCoordenada == 'X')
                        x = coordenadaAntiga;
                    else if (tipoCoordenada == 'y' || tipoCoordenada == 'Y')
                        y = coordenadaAntiga;
                    else
                        throw new Exception("Não foi informado um tipo de coordenada valida, apenas x ou y são aceitos (Está mensagem é para o programador)");

                    caminhoLivre = false; 
                    break;
                }
            }
            return caminhoLivre;
        }
        
        static void ExecutaExplosao(int x, int y)
        {
            bool continuaCim = true;
            bool continuaBai = true;
            bool continuaEsq = true;
            bool continuaDir = true;

            for (int i = 1; i <= explosaoRaio; i++) //Passa por todas as distancias de explosão
            {
                if (continuaCim)                                //Se a explosão ainda está espalhando
                    continuaCim = ExecutaExplosaoAux(x, y - i); //Um pra cada uma das 4 direções em que a explosão espalha
                if (continuaBai)
                    continuaBai = ExecutaExplosaoAux(x, y + i);
                if (continuaEsq)
                    continuaEsq = ExecutaExplosaoAux(x - i, y);
                if (continuaDir)
                    continuaDir = ExecutaExplosaoAux(x + i, y);
            }
        }

        static bool ExecutaExplosaoAux(int x, int y)
        {
            if (CaractereNaPosicao(x, y) == '▓')
            {
                coordenadas1dDestruicao.Add(Coordenada1D(x, y)); //Salva a coordenada da colisão
                return false;                                  //Para o espalhamento da explosão na primeira colisão
            }
            else if (CaractereNaPosicao(x, y) == '█')
                return false;
            else if (CaractereNaPosicao(x, y) == '@')
            {
                gameOver = true;
                return false;
            }
            else
            {
                coordenadas1dExplosao.Add(Coordenada1D(x, y));
                return true;
            }
            
        }

        static char CaractereNaPosicao(int x, int y)
        {
            return Convert.ToChar(background.Substring(Coordenada1D(x, y), 1));
        }

        static char CaractereNaPosicao(int c1D)
        {
            return Convert.ToChar(background.Substring(c1D, 1));
        }

        static void DesenhaCaracterePosicao(int x, int y, char caractere)
        {
            background = background.Remove(Coordenada1D(x, y), 1);
            background = background.Insert(Coordenada1D(x, y), $"{caractere}");
        }

        static void DesenhaCaracterePosicao(int c1d, char caractere)
        {
            background = background.Remove(c1d, 1);
            background = background.Insert(c1d, $"{caractere}");
        }

        static void GeraParedesObstaculos()
        {
            for(int i = 0; i < background.Length; i++)
            {
                Cartesiano ixy =  CoordenadaXY(i);
                if (background[i] == ' ' &&   !(Math.Abs(ixy.x - jogadorX) <= 1 && Math.Abs(ixy.y - jogadorY) <= 1) && Convert.ToBoolean(random.Next(0, 2)))
                {
                    DesenhaCaracterePosicao(i, '▓');
                }
            }
        }

        static void GeraPowerUpsSaida()
        {
            int r;
            do
            {
                r = random.Next(0, largTela * (altuTela - 3));
                if(CaractereNaPosicao(r) == '▓')
                {
                    c1dPowerUp = r;
                    break;
                }
            } while (true);

            do
            {
                r = random.Next(0, largTela * (altuTela - 3));
                if (r != c1dPowerUp && CaractereNaPosicao(r) == '▓')
                {
                    c1dSaida = r;
                    break;
                }
            } while (true);
        }

        static void GeraNivel()
        {
            nivelAtual++;

            Console.Clear();

            Console.SetCursorPosition((largTela / 2) - 4, altuTela / 2);
            Console.Write("Nível " + nivelAtual);

            Thread.Sleep(2000);

            background =
                "███████████████████████████████████" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "█ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █" +
                "█                                 █" +
                "███████████████████████████████████";

            GeraParedesObstaculos();
            GeraInimigos(5);
            GeraPowerUpsSaida();

            DesenhaCaracterePosicao((int)newJogadorX, (int)newJogadorY, '@');
            Console.SetCursorPosition(0, 0);
            Console.Write(background);
        }

        static void GeraInimigos(int quantInimigos)
        {
            int r;
            int inimigos = quantInimigos;
            while(inimigos > 0)
            {
                r = random.Next(0, largTela * (altuTela - 3));
                Cartesiano r2d = CoordenadaXY(r);
                if (CaractereNaPosicao(r) == ' ' && !(Math.Abs(r2d.x - jogadorX) <= 3 && Math.Abs(r2d.y - jogadorY) <= 3))
                {
                    DesenhaCaracterePosicao(r, '☺');
                    Inimigo inim = new Inimigo();
                    inim.posX = inim.novposX = r2d.x;
                    inim.posY = inim.novposY = r2d.y;
                    inim.direcaoMovimento = DirecoesCardeais.nulo;
                    listaInimigos.Enqueue(inim);
                    inimigos--;
                }
                
            }            
        }

        static Cartesiano CoordenadaXY(int posicao1d)
        {
            int y = Convert.ToInt32(Math.Truncate((double)posicao1d / (double)largTela));
            int x = posicao1d - (y * largTela);
            Cartesiano c = new Cartesiano();
            c.x = x;
            c.y = y;
            return c;
        }

        static void AsciiTableTest()
        {
            for (int i = 0; i <= 255; i++)
            {
                Console.WriteLine(i + " - " + Convert.ToChar(i));
            }
            Console.ReadLine();
        }
    }
}

/*
* "###################################" 35 caracteres
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "# # # # # # # # # # # # # # # # # #"
* "#                                 #"
* "###################################"
* "Score:"
* 20 Linhas
*/
/*
 * outros "sprites"
 * Jogador            @
 * Morto              X
 * Parede             █ (alt 219)
 * Parede Destrutivel ▓ (alt 178)
 * Bomba              Ó (alt 224)
 * Explosão           ☼ (alt 15)
 * PowerUp            ® (alt 169)
 * Saída              # 
 * Inimigo            ☺ (alt 1)
 *                    ☻ (alt 2)
 *                    ♥ (alt 3)
 */
