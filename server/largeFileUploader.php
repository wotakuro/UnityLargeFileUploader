<?PHP
ini_set('display_errors', "On");
main();

  function main(){
    $basePath = "./lagefiles";
    if(!empty($_FILES['uploaded_file']))
    {
      UploadPhase($basePath);
    }else{
      header('HTTP/1.0 403 Forbidden', FALSE);
    }
  }

  function InitializePhase(){
    $uniqueId = uniqid();

    print('{uniqueid:"'.$uniqueId .'"}';
  }

  function UploadPhase($basePath){
    $originTmp = $_FILES['uploaded_file']['tmp_name'];
    $originFile = $_FILES['uploaded_file']['name'];
    $isDone = ($_POST['block']+1 == $_POST['blockNum'] && $_POST['blockNum'] > 0);

    if(!executeFile($originTmp , $originFile,$basePath ,$isDone )) {
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
