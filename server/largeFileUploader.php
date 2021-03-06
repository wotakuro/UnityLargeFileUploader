<?PHP
ini_set('display_errors', "On");
main();

  function main(){
    $workingPath = "../workingDir";
    $finalPath = "../uploadFiles";

    switch($_POST['mode']){
      case 0:
        InitializePhase($workingPath);
        break;
      case 1:
        UploadPhase($workingPath,$finalPath);
        break;
    }
  }

  function InitializePhase($workingPath){
    $uniqueId = uniqid();
    $dirPath = $workingPath . '/'.$uniqueId;
    mkdir($dirPath );
    $fileInfo = $_POST['fileinfo'];
    file_put_contents( $dirPath.'/info.txt',$fileInfo);
    print('{"sessionid":"'.$uniqueId .'"}');
  }

  function UploadPhase($workingPath,$finalPath){
    if(empty($_FILES['uploaded_file']))
    {
      header('HTTP/1.0 403 Forbidden', FALSE);
      return;
    }

    $originTmp = $_FILES['uploaded_file']['tmp_name'];
    $originFile = $_FILES['uploaded_file']['name'];
    $isDone = ($_POST['block']+1 == $_POST['blockNum'] && $_POST['blockNum'] > 0);
    $path = $workingPath .'/' .$_POST['sessionid'] . '/';

    if(!executeFile($originTmp , $originFile,$path ,$isDone )) {
      header('HTTP/1.0 403 Forbidden', FALSE);
    }
  }

  function executeFile( $originTmpPath, $originPath, $dir , $isDone){
    $finalPath = $dir . basename($originPath);
    $tmpPath = $finalPath . ".tmp";
    $partFile = $finalPath.".part";
    if(!move_uploaded_file($originTmpPath, $tmpPath)) {
       return false;
    }

    $fp = fopen( $tmpPath , 'r' );
    $bin = fread( $fp , 2048 * 1024 );
    fclose($fp);
    // append file
    $apendfp = fopen($partFile , 'a' );
    fwrite($apendfp,$bin);
    fclose($apendfp);
    if($isDone){
      rename($partFile,$finalPath);
      chmod($finalPath,0777);
      unlink($tmpPath);
    }
    return true;
  }
?>
